using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.XUnit.TestHarness;

internal class BankImportUtilitiesTestHarness : BankImportUtilities
{
    public BankImportUtilitiesTestHarness(ILogger logger) : base(logger)
    {
    }

    public BankImportUtilitiesTestHarness() : base(new FakeLogger())
    {
    }

    public Action<string>? AbortIfFileDoesntExistOverride { get; set; }

    internal override void AbortIfFileDoesntExist(string fileName)
    {
        if (AbortIfFileDoesntExistOverride is null)
        {
            return;
        }

        AbortIfFileDoesntExistOverride(fileName);
    }
}
