using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class CsvOnDiskStatementModelRepositoryV1TestHarness : CsvOnDiskStatementModelRepositoryV1
    {
        public CsvOnDiskStatementModelRepositoryV1TestHarness([NotNull] IAccountTypeRepository accountTypeRepository, [NotNull] IUserMessageBox userMessageBox, [NotNull] IBudgetBucketRepository bucketRepository, [NotNull] BankImportUtilities importUtilities, [NotNull] ILogger logger)
            : base(accountTypeRepository, userMessageBox, bucketRepository, importUtilities, logger)
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