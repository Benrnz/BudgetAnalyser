using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Statement.Data;
using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Engine.UnitTest.TestHarness
{
    internal class CsvOnDiskStatementModelRepositoryV1TestHarness : CsvOnDiskStatementModelRepositoryV1
    {
        public CsvOnDiskStatementModelRepositoryV1TestHarness(BankImportUtilities importUtilities, IReaderWriterSelector readerWriterSelector)
            : base(importUtilities,
                new FakeLogger(),
                new Mapper_TransactionSetDto_StatementModel(
                    new FakeLogger(), 
                    new Mapper_TransactionDto_Transaction(new InMemoryAccountTypeRepository(), new BucketBucketRepoAlwaysFind(), new InMemoryTransactionTypeRepository())),
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
            if (ReadLinesOverride == null)
            {
                return Task.FromResult<IEnumerable<string>>(new List<string>());
            }

            return Task.FromResult(ReadLinesOverride(fileName));
        }

        protected override Task<IEnumerable<string>> ReadLinesAsync(string fileName, int lines, bool isEncrypted)
        {
            if (ReadLinesOverride == null)
            {
                return Task.FromResult<IEnumerable<string>>(new List<string>());
            }

            return Task.FromResult(ReadLinesOverride(fileName).Take(lines));
        }
    }
}