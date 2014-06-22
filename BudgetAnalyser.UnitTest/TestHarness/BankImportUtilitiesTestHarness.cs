using System;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Statement;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class BankImportUtilitiesTestHarness : BankImportUtilities
    {
        public BankImportUtilitiesTestHarness([NotNull] ILogger logger) : base(logger)
        {
        }

        public Action<string, IUserMessageBox> AbortIfFileDoesntExistOverride { get; set; }

        internal override void AbortIfFileDoesntExist(string fileName, IUserMessageBox messageBox)
        {
            if (AbortIfFileDoesntExistOverride == null)
            {
                return;
            }

            AbortIfFileDoesntExistOverride(fileName, messageBox);
        }
    }
}
