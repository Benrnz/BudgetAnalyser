using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Statement.Data;

namespace BudgetAnalyser.Engine.UnitTest.TestHarness;

internal class CsvOnDiskStatementModelRepositoryV1TestHarness : CsvOnDiskStatementModelRepositoryV1
{
    public CsvOnDiskStatementModelRepositoryV1TestHarness(BankImportUtilities importUtilities, IReaderWriterSelector readerWriterSelector)
        : base(importUtilities,
            new FakeLogger(),
            new MapperStatementModelToDto2(new InMemoryAccountTypeRepository(), new BucketBucketRepoAlwaysFind(), new InMemoryTransactionTypeRepository(), new FakeLogger()),
            readerWriterSelector)
    {
    }

    public CsvOnDiskStatementModelRepositoryV1TestHarness(
        BankImportUtilities importUtilities,
        IDtoMapper<TransactionSetDto, StatementModel> mapper,
        IReaderWriterSelector readerWriterSelector)
        : base(importUtilities,
            new FakeLogger(),
            mapper,
            readerWriterSelector)
    {
    }

    public Func<string, IEnumerable<string>> ReadLinesOverride { get; set; }

    protected override Task<IEnumerable<string>> ReadLinesAsync(string fileName, bool isEncrypted)
    {
        return ReadLinesOverride is null
            ? Task.FromResult<IEnumerable<string>>(new List<string>())
            : Task.FromResult(ReadLinesOverride(fileName));
    }

    protected override Task<IEnumerable<string>> ReadLinesAsync(string fileName, int lines, bool isEncrypted)
    {
        return ReadLinesOverride is null
            ? Task.FromResult<IEnumerable<string>>(new List<string>())
            : Task.FromResult(ReadLinesOverride(fileName).Take(lines));
    }

    internal async Task WriteToStreamTest(TransactionSetDto dto, StreamWriter writer)
    {
        await WriteToStream(dto, writer);
    }
}
