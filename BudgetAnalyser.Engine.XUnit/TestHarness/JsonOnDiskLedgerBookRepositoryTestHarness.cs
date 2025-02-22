using System.Text;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.XUnit.TestHarness;

internal class JsonOnDiskLedgerBookRepositoryTestHarness : JsonOnDiskLedgerBookRepository
{
    public JsonOnDiskLedgerBookRepositoryTestHarness(
        IDtoMapper<LedgerBookDto, LedgerBook> mapper,
        BankImportUtilities importUtilities,
        IReaderWriterSelector readerWriterSelector,
        ILogger logger) : base(mapper, importUtilities, readerWriterSelector, logger)
    {
    }

    public string SerialisedData { get; private set; }

    protected override async Task SerialiseAndWriteToStream(Stream stream, LedgerBookDto dataEntity)
    {
        await base.SerialiseAndWriteToStream(stream, dataEntity);
        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var result = await reader.ReadToEndAsync();
        SerialisedData = result;
    }
}
