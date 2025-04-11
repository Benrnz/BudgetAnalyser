using System.Text;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Matching.Data;

namespace BudgetAnalyser.Engine.XUnit.TestHarness;

internal class JsonOnDiskMatchingRuleRepositoryTestHarness : JsonOnDiskMatchingRuleRepository
{
    private readonly EmbeddedResourceFileReaderWriterEncrypted? encryptedReaderWriter;

    public JsonOnDiskMatchingRuleRepositoryTestHarness(ILogger logger, IReaderWriterSelector readerWriterSelector, IBudgetBucketRepository bucketRepo)
        : base(new MapperMatchingRuleToDto2(bucketRepo), logger, readerWriterSelector)
    {
        Dto = Array.Empty<MatchingRuleDto>();
        SerialisedData = string.Empty;
        SerialisedBytes = [];
        if (readerWriterSelector.SelectReaderWriter(true) is EmbeddedResourceFileReaderWriterEncrypted encryptor)
        {
            this.encryptedReaderWriter = encryptor;
        }
    }

    public IEnumerable<MatchingRuleDto> Dto { get; set; }

    public bool IsEncryptedAtLastAccess { get; private set; }

    public byte[] SerialisedBytes { get; private set; }

    public string SerialisedData { get; private set; }

    protected override async Task<List<MatchingRuleDto>> LoadFromDiskAsync(string fileName, bool isEncrypted)
    {
        Dto = await base.LoadFromDiskAsync(fileName, isEncrypted);
        return Dto.ToList();
    }

    protected override IEnumerable<MatchingRuleDto> MapToDto(IEnumerable<MatchingRule> widgets)
    {
        Dto = base.MapToDto(widgets);
        return Dto;
    }

    protected override Task SaveToDiskAsync(string fileName, IEnumerable<MatchingRuleDto> dataEntities, bool isEncrypted)
    {
        IsEncryptedAtLastAccess = isEncrypted;
        return base.SaveToDiskAsync(fileName, dataEntities, isEncrypted);
    }

    protected override async Task SerialiseAndWriteToStream(Stream stream, IEnumerable<MatchingRuleDto> dataEntities)
    {
        await base.SerialiseAndWriteToStream(stream, dataEntities);
        stream.Position = 0;
        if (IsEncryptedAtLastAccess)
        {
            var encryptedDestinationStream = this.encryptedReaderWriter!.OutputStream;
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
