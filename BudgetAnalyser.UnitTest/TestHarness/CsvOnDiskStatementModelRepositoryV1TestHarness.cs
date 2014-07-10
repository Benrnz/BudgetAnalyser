using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Statement.Data;
using BudgetAnalyser.UnitTest.Account;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class CsvOnDiskStatementModelRepositoryV1TestHarness : CsvOnDiskStatementModelRepositoryV1
    {
        public CsvOnDiskStatementModelRepositoryV1TestHarness(IAccountTypeRepository accountTypeRepo, IUserMessageBox userMessageBox, IBudgetBucketRepository bucketRepo, BankImportUtilities importUtilities, ILogger logger)
            : base(userMessageBox, 
                    importUtilities, 
                    logger, 
                    new TransactionSetDtoToStatementModelMapper(
                        logger, 
                        new TransactionDtoToTransactionMapper(accountTypeRepo, bucketRepo, new InMemoryTransactionTypeRepository())), 
                    new StatementModelToTransactionSetDtoMapper(new TransactionToTransactionDtoMapper()))
        {
        }

        public Func<string, IEnumerable<string>> ReadLinesOverride { get; set; }

        protected override IEnumerable<string> ReadLines(string fileName)
        {
            if (ReadLinesOverride == null)
            {
                return new List<string>();
            }

            return ReadLinesOverride(fileName);
        }

        protected override IEnumerable<string> ReadLines(string fileName, int lines)
        {
            if (ReadLinesOverride == null)
            {
                return new List<string>();
            }

            return ReadLinesOverride(fileName).Take(lines);
        }
    }
}