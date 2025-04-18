using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Statement.Data;

namespace BudgetAnalyser.Engine.XUnit.TestHarness;

internal class CsvOnDiskStatementModelRepositoryV2TestHarness : CsvOnDiskStatementModelRepositoryV2
{
    public CsvOnDiskStatementModelRepositoryV2TestHarness(
        ILogger logger,
        IDtoMapper<TransactionSetDto, StatementModel> mapper,
        IReaderWriterSelector readerWriterSelector)
        : base(new BankImportUtilitiesTestHarness(), logger, mapper, readerWriterSelector)
    {
    }

    public TransactionSetDto? Dto { get; set; }
    public string SerialisedData { get; set; }

    protected override TransactionSetDto MapToDto(StatementModel model)
    {
        if (Dto is null)
        {
            Dto = base.MapToDto(model);
            return Dto;
        }

        return Dto;
    }

    protected override async Task WriteToStream(TransactionSetDto transactionSet, StreamWriter streamWriter)
    {
        await base.WriteToStream(transactionSet, streamWriter);
        streamWriter.BaseStream.Position = 0;
        using var reader = new StreamReader(streamWriter.BaseStream);
        SerialisedData = await reader.ReadToEndAsync();
    }
}
