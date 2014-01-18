using System;
using System.Linq;
using BudgetAnalyser.Engine.Ledger;

namespace BudgetAnalyser.UnitTest.Helper
{
    public static class LedgerBookHelper
    {
        public static void Output(this LedgerBook book, bool outputTransactions = false)
        {
            Console.WriteLine("Name: {0}", book.Name);
            Console.WriteLine("Filename: {0}", book.FileName);
            Console.WriteLine("Modified: {0}", book.Modified);
            Console.Write("Date        ");
            foreach (var ledger in book.Ledgers)
            {
                Console.Write("{0}", ledger.BudgetBucket.Code.PadRight(18));
            }
            
            Console.Write("Surplus  BankBalance  Adjustments LedgerBalance");
            Console.WriteLine();
            Console.WriteLine("====================================================================================================================");

            foreach (var line in book.DatedEntries)
            {
                Console.Write("{0:d}  ", line.Date);
                foreach (var entry in line.Entries)
                {
                    Console.Write("{0} {1} ", entry.NetAmount.ToString("N").PadRight(8), entry.Balance.ToString("N").PadRight(8));
                }

                Console.Write(line.CalculatedSurplus.ToString("N").PadRight(9));
                Console.Write(line.BankBalance.ToString("N").PadRight(13));
                Console.Write(line.TotalBalanceAdjustments.ToString("N").PadRight(12));
                Console.Write(line.LedgerBalance.ToString("N").PadRight(9));
                Console.WriteLine();

                if (outputTransactions)
                {
                    foreach (var entry in line.Entries)
                    {
                        foreach (var transaction in entry.Transactions)
                        {
                            Console.WriteLine("          {0} {1} {2} {3}", 
                                entry.Ledger.BudgetBucket.Code.PadRight(6), 
                                (transaction.Credit.ToString("N") + "Cr").PadRight(8), 
                                (transaction.Debit.ToString("N") + "Dr").PadRight(8), 
                                transaction.Narrative);
                        }
                    }
                    Console.WriteLine("====================================================================================================================");
                }
            }
        }
    }
}
