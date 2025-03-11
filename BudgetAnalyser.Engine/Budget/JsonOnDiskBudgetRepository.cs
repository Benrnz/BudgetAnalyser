using System.Text.Json;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Services;

namespace BudgetAnalyser.Engine.Budget;

/// <summary>
///     A repository to store the budget collections on local disk as a Json file.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Budget.IBudgetRepository" />
[AutoRegisterWithIoC(SingleInstance = true)]
public class JsonOnDiskBudgetRepository : IBudgetRepository
{
    private readonly IBudgetBucketRepository budgetBucketRepository;
    private readonly IDtoMapper<BudgetCollectionDto, BudgetCollection> mapper;
    private readonly IReaderWriterSelector readerWriterSelector;
    private BudgetCollection currentBudgetCollection;
    private bool isEncryptedAtLastAccess;

    /// <summary>
    ///     Initializes a new instance of the <see cref="JsonOnDiskBudgetRepository" /> class.
    /// </summary>
    public JsonOnDiskBudgetRepository(IBudgetBucketRepository bucketRepository, IDtoMapper<BudgetCollectionDto, BudgetCollection> mapper, IReaderWriterSelector readerWriterSelector)
    {
        this.budgetBucketRepository = bucketRepository ?? throw new ArgumentNullException(nameof(bucketRepository));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.readerWriterSelector = readerWriterSelector ?? throw new ArgumentNullException(nameof(readerWriterSelector));
        var budget = new BudgetModel();
        this.currentBudgetCollection = new BudgetCollection(budget);
        this.budgetBucketRepository.Initialise(new List<BudgetBucketDto>());
    }

    /// <summary>
    ///     Creates a new empty <see cref="BudgetCollection" /> but does not save it.
    /// </summary>
    public BudgetCollection CreateNew()
    {
        var budget = new BudgetModel();
        this.currentBudgetCollection = new BudgetCollection(budget);
        this.budgetBucketRepository.Initialise(new List<BudgetBucketDto>());
        return this.currentBudgetCollection;
    }

    public async Task<BudgetCollection> CreateNewAndSaveAsync(string storageKey)
    {
        if (storageKey.IsNothing())
        {
            throw new ArgumentNullException(nameof(storageKey));
        }

        var newBudget = new BudgetModel { EffectiveFrom = DateTime.Today, Name = Path.GetFileNameWithoutExtension(storageKey).Replace('.', ' ') };

        this.currentBudgetCollection = new BudgetCollection(newBudget) { StorageKey = storageKey };

        this.budgetBucketRepository.Initialise(new List<BudgetBucketDto>());

        await SaveAsync(storageKey, false);
        return this.currentBudgetCollection;
    }

    public async Task<BudgetCollection> LoadAsync(string storageKey, bool isEncrypted)
    {
        if (storageKey.IsNothing())
        {
            throw new ArgumentNullException(nameof(storageKey));
        }

        this.isEncryptedAtLastAccess = isEncrypted;
        var reader = this.readerWriterSelector.SelectReaderWriter(isEncrypted);

        if (!reader.FileExists(storageKey))
        {
            throw new KeyNotFoundException("File not found. " + storageKey);
        }

        BudgetCollectionDto? dto;
        try
        {
            dto = await LoadJsonFromDiskAsync(storageKey, isEncrypted);
        }
        catch (JsonException ex)
        {
            throw new DataFormatException($"The budget file '{storageKey}' is an invalid format. This is probably due to changes in the code, most likely namespace changes.", ex);
        }
        catch (EncryptionKeyIncorrectException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new DataFormatException("Deserialisation the Budget file failed, an exception was thrown by the Json deserialiser, the file format is invalid.", ex);
        }

        if (dto is null)
        {
            throw new DataFormatException($"The Budget file ({storageKey}) is not in the correct format. It may have been tampered with.");
        }

        // Bucket Repository must be initialised first, the budget model incomes/expenses are dependent on the bucket repository.
        this.budgetBucketRepository.Initialise(dto.Buckets);

        var budgetCollection = this.mapper.ToModel(dto);
        budgetCollection.StorageKey = storageKey;
        this.currentBudgetCollection = budgetCollection;
        return budgetCollection;
    }

    public async Task SaveAsync()
    {
        if (this.currentBudgetCollection is null)
        {
            throw new InvalidOperationException("There is no current budget collection loaded.");
        }

        await SaveAsync(this.currentBudgetCollection.StorageKey, this.isEncryptedAtLastAccess);
    }

    public async Task SaveAsync(string storageKey, bool isEncrypted)
    {
        if (this.currentBudgetCollection is null)
        {
            throw new InvalidOperationException("There is no current budget collection loaded.");
        }

        this.currentBudgetCollection.StorageKey = storageKey;
        var dto = MapToDto();
        await SaveDtoToDiskAsync(dto, isEncrypted);
        this.isEncryptedAtLastAccess = isEncrypted;
    }

    protected virtual async Task<BudgetCollectionDto> LoadJsonFromDiskAsync(string fileName, bool isEncrypted)
    {
        var reader = this.readerWriterSelector.SelectReaderWriter(isEncrypted);
        await using var stream = reader.CreateReadableStream(fileName);
        var dto = await JsonSerializer.DeserializeAsync<BudgetCollectionDto>(stream);

        return dto ?? throw new DataFormatException("Unable to deserialise Budget into correct type. File is corrupt.");
    }

    protected virtual BudgetCollectionDto MapToDto()
    {
        return this.mapper.ToDto(this.currentBudgetCollection);
    }

    protected virtual async Task SaveDtoToDiskAsync(BudgetCollectionDto dataEntity, bool isEncrypted)
    {
        if (dataEntity is null)
        {
            throw new ArgumentNullException(nameof(dataEntity));
        }

        var writer = this.readerWriterSelector.SelectReaderWriter(isEncrypted);
        await using var stream = writer.CreateWritableStream(dataEntity.StorageKey);
        await SerialiseAndWriteToStream(stream, dataEntity);
    }

    protected virtual async Task SerialiseAndWriteToStream(Stream stream, BudgetCollectionDto dataEntity)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        await JsonSerializer.SerializeAsync(stream, dataEntity, options);
    }
}
