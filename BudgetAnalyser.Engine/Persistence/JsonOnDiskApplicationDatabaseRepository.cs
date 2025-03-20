using System.Text.Json;
using BudgetAnalyser.Engine.Ledger.Data;

namespace BudgetAnalyser.Engine.Persistence;

/// <summary>
///     A repository to store the top level Budget Analyser object graph in Xaml format on local disk.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Persistence.IApplicationDatabaseRepository" />
// [AutoRegisterWithIoC]
public class JsonOnDiskApplicationDatabaseRepository : IApplicationDatabaseRepository
{
    private readonly ILogger logger;
    private readonly IDtoMapper<BudgetAnalyserStorageRoot, ApplicationDatabase> mapper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="JsonOnDiskApplicationDatabaseRepository" /> class.
    /// </summary>
    public JsonOnDiskApplicationDatabaseRepository(IDtoMapper<BudgetAnalyserStorageRoot, ApplicationDatabase> mapper, ILogger logger)
    {
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<ApplicationDatabase> CreateNewAsync(string storageKey)
    {
        if (storageKey.IsNothing())
        {
            throw new ArgumentNullException(nameof(storageKey));
        }

        var path = LocateFilePath(storageKey);
        var storageRoot = new BudgetAnalyserStorageRoot
        {
            BudgetCollectionRootDto = new StorageBranch { Source = path + ".Budget.json" },
            LedgerBookRootDto = new StorageBranch { Source = path + ".LedgerBook.json" },
            LedgerReconciliationToDoCollection = new List<ToDoTaskDto>(),
            MatchingRulesCollectionRootDto = new StorageBranch { Source = path + ".MatchingRules.json" },
            StatementModelRootDto = new StorageBranch { Source = path + ".Transactions.csv" },
            WidgetCollectionRootDto = new StorageBranch { Source = path + ".Widgets.json" },
            IsEncrypted = false
        };

        var appDb = this.mapper.ToModel(storageRoot);
        appDb.FileName = storageKey;

        await SaveAsync(appDb);

        return appDb;
    }

    /// <inheritdoc />
    public async Task<ApplicationDatabase> LoadAsync(string storageKey)
    {
        if (storageKey.IsNothing())
        {
            throw new ArgumentNullException(nameof(storageKey));
        }

        if (!FileExists(storageKey))
        {
            this.logger.LogError(_ => $"Application database file '{storageKey}' does not exist.");
            throw new KeyNotFoundException("File does not exist.");
        }

        BudgetAnalyserStorageRoot dto;
        try
        {
            dto = await LoadJsonFromDiskAsync(storageKey);
        }
        catch (Exception ex)
        {
            this.logger.LogError(_ => $"Deserialisation of Application Database file failed, an exception was thrown by the Json deserialiser, the file format is invalid.\n\r {ex}");
            throw new DataFormatException("Deserialisation of Application Database file failed, an exception was thrown by the Json deserialiser, the file format is invalid.", ex);
        }

        var db = this.mapper.ToModel(dto);
        db.FileName = storageKey;
        return db;
    }

    /// <inheritdoc />
    public async Task SaveAsync(ApplicationDatabase budgetAnalyserDatabase)
    {
        if (budgetAnalyserDatabase is null)
        {
            throw new ArgumentNullException(nameof(budgetAnalyserDatabase));
        }

        if (string.IsNullOrWhiteSpace(budgetAnalyserDatabase.FileName))
        {
            throw new ArgumentException("Database filename is null and has not been set.");
        }

        var dto = this.mapper.ToDto(budgetAnalyserDatabase);
        await SaveDtoToDisk(dto, budgetAnalyserDatabase.FileName);
    }

    protected virtual Stream CreateReadableStream(string fileName)
    {
        return new FileStream(fileName, FileMode.Open, FileAccess.Read);
    }

    protected virtual Stream CreateWritableStream(string fileName)
    {
        return new FileStream(fileName, FileMode.Create);
    }

    protected virtual bool FileExists(string budgetAnalyserDataStorage)
    {
        return File.Exists(budgetAnalyserDataStorage);
    }

    protected virtual string LocateFilePath(string storageKey)
    {
        return Path.Combine(Path.GetDirectoryName(storageKey) ?? string.Empty, Path.GetFileNameWithoutExtension(storageKey));
    }

    protected virtual async Task SaveDtoToDisk(BudgetAnalyserStorageRoot dto, string storageKey)
    {
        if (dto is null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        await using var stream = CreateWritableStream(storageKey);
        await SerialiseAndWriteToStream(stream, dto);
    }

    protected virtual async Task SerialiseAndWriteToStream(Stream stream, BudgetAnalyserStorageRoot dto)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        await JsonSerializer.SerializeAsync(stream, dto, options);
    }

    private async Task<BudgetAnalyserStorageRoot> LoadJsonFromDiskAsync(string fileName)
    {
        await using var stream = CreateReadableStream(fileName);
        var dto = await JsonSerializer.DeserializeAsync<BudgetAnalyserStorageRoot>(stream);

        return dto ?? throw new DataFormatException("Unable to deserialise Application Database file. File is corrupt.");
    }
}
