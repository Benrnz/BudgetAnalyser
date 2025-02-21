using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.XUnit.TestHarness;

internal class BankImportUtilitiesTestHarness() : BankImportUtilities(new FakeLogger())
{
    public Action<string> AbortIfFileDoesntExistOverride { get; set; }

    internal override void AbortIfFileDoesntExist(string fileName)
    {
        if (AbortIfFileDoesntExistOverride is null)
        {
            return;
        }

        AbortIfFileDoesntExistOverride(fileName);
    }
}
