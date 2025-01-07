using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.UnitTest.Helper;

public static class StatementModelHelper
{
    public static void Output(this StatementModel instance, DateTime startDate, IReesTestOutput? outputWriter = null)
    {
        var writer = NonNullableOutputWriter(outputWriter);
        writer.WriteLine(string.Empty);
        writer.WriteLine("******************************************************** STATEMENT OUTPUT ***********************************************************");
        writer.WriteLine("Date       Description     Bucket     Reference1      Reference2          Amount Account         Id");
        writer.WriteLine("=====================================================================================================================================");
        foreach (var transaction in instance.AllTransactions.Where(t => t.Date >= startDate).OrderBy(t => t.Date))
        {
            writer.WriteLine(
                "{0} {1} {2} {3} {4} {5} {6} {7}",
                transaction.Date.ToString("d-MMM-yy").PadRight(10),
                transaction.Description.Truncate(15).PadRight(15),
                transaction.BudgetBucket.Code.PadRight(10) ?? string.Empty.PadRight(10),
                transaction.Reference1.Truncate(15).PadRight(15),
                transaction.Reference2.Truncate(15).PadRight(15),
                transaction.Amount.ToString("N").PadLeft(10),
                transaction.Account.Name.PadRight(15),
                transaction.Id);
        }

        writer.WriteLine("=====================================================================================================================================");
        writer.WriteLine($"Total Transactions: {instance.AllTransactions.Count()}");
        writer.WriteLine(string.Empty);
    }

    private static IReesTestOutput NonNullableOutputWriter(IReesTestOutput? outputWriter)
    {
        return outputWriter ?? new DebugTestOutput();
    }
}
