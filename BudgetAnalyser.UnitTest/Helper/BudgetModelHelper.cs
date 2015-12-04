using System;
using System.Diagnostics;
using System.Linq;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.UnitTest.Helper
{
    public static class BudgetModelHelper
    {
        public static void Output(this IBudgetCurrencyContext instance)
        {
            Console.WriteLine();
            Console.WriteLine($"Budget Currency Context: {instance.FileName} Budget Name: {instance.Model.Name} Effective From: {instance.Model.EffectiveFrom:d} Effective Until: {instance.EffectiveUntil:d}");
            if (instance.BudgetActive)
            {
                Console.WriteLine("Budget is ACTIVE.");
            } else if (instance.BudgetArchived)
            {
                Console.WriteLine("Budget is ARCHIVED.");
            } else if (instance.BudgetInFuture)
            {
                Console.WriteLine("Budget is FUTURE.");
            }
            instance.Model.Output(false);
        }

        public static void Output(this BudgetModel instance, bool includeTitle = true)
        {
            if (includeTitle)
            {
                Console.WriteLine();
                Console.WriteLine($"Budget Model: '{instance.Name}' EffectiveFrom: {instance.EffectiveFrom}");
            }
            Console.WriteLine(@"    Incomes                      Expenses");
            Console.WriteLine(@"    ==================================================================");
            int incomeIndex = 0;
            var incomeArray = instance.Incomes.ToArray();
            foreach (Expense expense in instance.Expenses)
            {
                if (incomeIndex <= incomeArray.GetUpperBound(0))
                {
                    Console.Write($"    {incomeArray[incomeIndex].Bucket.Code.PadRight(10)} {incomeArray[incomeIndex].Amount:F2}");
                    Console.WriteLine($"           {expense.Bucket.Code.PadRight(10)} {expense.Amount:F2}");
                    incomeIndex++;
                }
                else
                {
                    Console.WriteLine($"                                 {expense.Bucket.Code.PadRight(10)} {expense.Amount:F2}");
                }
            }

            Console.WriteLine(@"    ------------------------------------------------------------------");
            Console.WriteLine($"    Total Income: {instance.Incomes.Sum(i => i.Amount):F2}        Total Expenses: {instance.Expenses.Sum(e => e.Amount):F2}");
            Console.WriteLine($"    Surplus: {instance.Surplus:F2}");
            Console.WriteLine(@"======================================================================");
        }
    }
}
