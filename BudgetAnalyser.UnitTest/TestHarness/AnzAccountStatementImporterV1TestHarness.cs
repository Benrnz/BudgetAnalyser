using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Statement;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class AnzAccountStatementImporterV1TestHarness : AnzAccountStatementImporterV1
    {
        public AnzAccountStatementImporterV1TestHarness([NotNull] IUserMessageBox userMessageBox, [NotNull] BankImportUtilities importUtilities, [NotNull] ILogger logger)
            : base(userMessageBox, importUtilities, logger)
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
    }
}
