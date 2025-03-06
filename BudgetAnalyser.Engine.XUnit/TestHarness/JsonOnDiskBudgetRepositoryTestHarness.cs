using System.Text;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.XUnit.TestHarness;

public class JsonOnDiskBudgetRepositoryTestHarness(
    IBudgetBucketRepository bucketRepository,
    IDtoMapper<BudgetCollectionDto, BudgetCollection> mapper,
    IReaderWriterSelector readerWriterSelector)
    : JsonOnDiskBudgetRepository(bucketRepository, mapper, readerWriterSelector)
{
    public BudgetCollectionDto Dto { get; set; }

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

    protected override async Task SerialiseAndWriteToStream(Stream stream, BudgetCollectionDto dataEntity)
    {
        await base.SerialiseAndWriteToStream(stream, dataEntity);
        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var result = await reader.ReadToEndAsync();
        SerialisedData = result;
    }
}
