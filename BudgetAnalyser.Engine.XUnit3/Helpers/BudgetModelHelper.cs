using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.XUnit.Helpers;

public static class BudgetModelHelper
{
    public static void Output(this IBudgetCurrencyContext instance, IReesTestOutput outputWriter = null)
    {
        var writer = NonNullableOutputWriter(outputWriter);
        writer.WriteLine(string.Empty);
        writer.WriteLine("******************************* BUDGET OUTPUT ********************************");
        writer.WriteLine(
            $"Budget Currency Context: {instance.StorageKey} Budget Name: {instance.Model.Name} Effective From: {instance.Model.EffectiveFrom:d} Effective Until: {instance.EffectiveUntil:d}");
        if (instance.BudgetActive)
        {
            writer.WriteLine("{0} Budget is ACTIVE.", instance.Model.BudgetCycle);
        }
        else if (instance.BudgetArchived)
        {
            writer.WriteLine("{0} Budget is ARCHIVED.", instance.Model.BudgetCycle);
        }
        else if (instance.BudgetInFuture)
        {
            writer.WriteLine("{0} Budget is FUTURE.", instance.Model.BudgetCycle);
        }

        instance.Model.Output(false, writer);
    }

    public static void Output(this BudgetModel instance, bool includeTitle = true, IReesTestOutput outputWriter = null)
    {
        var writer = NonNullableOutputWriter(outputWriter);
        if (includeTitle)
        {
            writer.WriteLine(string.Empty);
            writer.WriteLine($"Budget Model: '{instance.Name}' EffectiveFrom: {instance.EffectiveFrom}");
        }

        writer.WriteLine(@"    Incomes                      Expenses");
        writer.WriteLine(@"    ==================================================================");
        var incomeIndex = 0;
        var incomeArray = instance.Incomes.ToArray();
        foreach (var expense in instance.Expenses)
        {
            if (incomeIndex <= incomeArray.GetUpperBound(0))
            {
                writer.Write($"    {incomeArray[incomeIndex].Bucket.Code,-10} {incomeArray[incomeIndex].Amount:F2}");
                writer.WriteLine($"           {expense.Bucket.Code,-10} {expense.Amount:F2}");
                incomeIndex++;
            }
            else
            {
                writer.WriteLine($"                                 {expense.Bucket.Code,-10} {expense.Amount:F2}");
            }
        }

        writer.WriteLine(@"    ------------------------------------------------------------------");
        writer.WriteLine($"    Total Income: {instance.Incomes.Sum(i => i.Amount):F2}        Total Expenses: {instance.Expenses.Sum(e => e.Amount):F2}");
        writer.WriteLine($"    Surplus: {instance.Surplus:F2}");
        writer.WriteLine(@"======================================================================");
    }

    private static IReesTestOutput NonNullableOutputWriter(IReesTestOutput outputWriter)
    {
        return outputWriter ?? new DebugTestOutput();
    }
}
