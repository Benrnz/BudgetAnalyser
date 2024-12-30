using System;
using System.Diagnostics;
using System.Linq;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.UnitTest.Helper
{
    public static class BudgetModelHelper
    {
        public static void Output(this IBudgetCurrencyContext instance)
        {
            Debug.WriteLine(string.Empty);
            Debug.WriteLine($"Budget Currency Context: {instance.FileName} Budget Name: {instance.Model.Name} Effective From: {instance.Model.EffectiveFrom:d} Effective Until: {instance.EffectiveUntil:d}");
            if (instance.BudgetActive)
            {
                Debug.WriteLine("Budget is ACTIVE.");
            }
            else if (instance.BudgetArchived)
            {
                Debug.WriteLine("Budget is ARCHIVED.");
            }
            else if (instance.BudgetInFuture)
            {
                Debug.WriteLine("Budget is FUTURE.");
            }
            instance.Model.Output(false);
        }

        public static void Output(this BudgetModel instance, bool includeTitle = true)
        {
            if (includeTitle)
            {
                Debug.WriteLine(string.Empty);
                Debug.WriteLine($"Budget Model: '{instance.Name}' EffectiveFrom: {instance.EffectiveFrom}");
            }
            Debug.WriteLine(@"    Incomes                      Expenses");
            Debug.WriteLine(@"    ==================================================================");
            int incomeIndex = 0;
            var incomeArray = instance.Incomes.ToArray();
            foreach (Expense expense in instance.Expenses)
            {
                if (incomeIndex <= incomeArray.GetUpperBound(0))
                {
                    Debug.Write($"    {incomeArray[incomeIndex].Bucket.Code.PadRight(10)} {incomeArray[incomeIndex].Amount:F2}");
                    Debug.WriteLine($"           {expense.Bucket.Code.PadRight(10)} {expense.Amount:F2}");
                    incomeIndex++;
                }
                else
                {
                    Debug.WriteLine($"                                 {expense.Bucket.Code.PadRight(10)} {expense.Amount:F2}");
                }
            }

            Debug.WriteLine(@"    ------------------------------------------------------------------");
            Debug.WriteLine($"    Total Income: {instance.Incomes.Sum(i => i.Amount):F2}        Total Expenses: {instance.Expenses.Sum(e => e.Amount):F2}");
            Debug.WriteLine($"    Surplus: {instance.Surplus:F2}");
            Debug.WriteLine(@"======================================================================");
        }
    }
}