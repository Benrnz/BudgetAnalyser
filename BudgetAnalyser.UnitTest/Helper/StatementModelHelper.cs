using System;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.UnitTest.Helper
{
    public static class StatementModelHelper
    {
        public static void Output(this StatementModel instance, DateTime startDate)
        {
            Console.WriteLine("Date       Description     Bucket     Reference1          Amount Account         Id");
            Console.WriteLine("==============================================================================================================");
            foreach (var transaction in instance.AllTransactions.Where(t => t.Date >= startDate).OrderBy(t => t.Date))
            {
                Console.WriteLine(
                    "{0} {1} {2} {3} {4} {5} {6}",
                    transaction.Date.ToString("d-MMM-yy").PadRight(10),
                    transaction.Description.Truncate(15).PadRight(15),
                    transaction.BudgetBucket.Code.PadRight(10),
                    transaction.Reference1.Truncate(15).PadRight(15),
                    transaction.Amount.ToString("N").PadLeft(10),
                    transaction.Account.Name.PadRight(15),
                    transaction.Id);
            }

            Console.WriteLine("==============================================================================================================");
            Console.WriteLine();
        }
    }
}
