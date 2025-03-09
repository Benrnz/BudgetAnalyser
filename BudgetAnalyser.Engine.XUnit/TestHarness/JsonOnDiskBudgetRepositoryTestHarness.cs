using System.Text;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.XUnit.TestHarness;

public class JsonOnDiskBudgetRepositoryTestHarness : JsonOnDiskBudgetRepository
{
    private readonly EmbeddedResourceFileReaderWriterEncrypted encryptedReaderWriter;

    public JsonOnDiskBudgetRepositoryTestHarness(IBudgetBucketRepository bucketRepository,
        IDtoMapper<BudgetCollectionDto, BudgetCollection> mapper,
        IReaderWriterSelector readerWriterSelector) : base(bucketRepository, mapper, readerWriterSelector)
    {
        if (readerWriterSelector.SelectReaderWriter(true) is EmbeddedResourceFileReaderWriterEncrypted encrypter)
        {
            this.encryptedReaderWriter = encrypter;
        }
    }

    public BudgetCollectionDto Dto { get; set; }

    public bool IsEncryptedAtLastAccess { get; private set; }

    public byte[] SerialisedBytes { get; private set; }

    public string SerialisedData { get; private set; }

    protected override async Task<BudgetCollectionDto> LoadJsonFromDiskAsync(string fileName, bool isEncrypted)
    {
        Dto = await base.LoadJsonFromDiskAsync(fileName, isEncrypted);
        return Dto;
    }

    protected override BudgetCollectionDto MapToDto(BudgetCollection book)
    {
        Dto = base.MapToDto(book);
        return Dto;
    }

    protected override Task SaveDtoToDiskAsync(BudgetCollectionDto dataEntity, bool isEncrypted)
    {
        IsEncryptedAtLastAccess = isEncrypted;
        return base.SaveDtoToDiskAsync(dataEntity, isEncrypted);
    }

    protected override async Task SerialiseAndWriteToStream(Stream stream, BudgetCollectionDto dataEntity)
    {
        await base.SerialiseAndWriteToStream(stream, dataEntity);
        stream.Position = 0;
        if (IsEncryptedAtLastAccess)
        {
            var encryptedDestinationStream = this.encryptedReaderWriter.OutputStream;
            encryptedDestinationStream.Position = 0;
            using var byteReader = new BinaryReader(encryptedDestinationStream);
            SerialisedBytes = byteReader.ReadBytes((int)encryptedDestinationStream.Length);
            SerialisedData = string.Empty;
        }
        else
        {
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var result = await reader.ReadToEndAsync();
            SerialisedData = result;
            SerialisedBytes = [];
        }
    }
}
