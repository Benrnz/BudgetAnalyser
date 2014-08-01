using System;
using System.Linq;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;

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
                Console.Write(line.TotalBankBalance.ToString("N").PadRight(13));
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
                                entry.LedgerColumn.BudgetBucket.Code.PadRight(6), 
                                (transaction.Credit.ToString("N") + "Cr").PadRight(8), 
                                (transaction.Debit.ToString("N") + "Dr").PadRight(8), 
                                transaction.Narrative);
                        }
                    }
                    Console.WriteLine("====================================================================================================================");
                }
            }
        }

        public static void Output(this LedgerBookDto book, bool outputTransactions = false)
        {
            Console.WriteLine("Name: {0}", book.Name);
            Console.WriteLine("Filename: {0}", book.FileName);
            Console.WriteLine("Modified: {0}", book.Modified);
            Console.Write("Date        ");
            foreach (var ledger in book.DatedEntries.SelectMany(l => l.Entries.Select(e => e.BucketCode)))
            {
                Console.Write("{0}", ledger.PadRight(18));
            }

            Console.Write("Surplus  BankBalance  Adjustments LedgerBalance");
            Console.WriteLine();
            Console.WriteLine("====================================================================================================================");

            foreach (var line in book.DatedEntries)
            {
                Console.Write("{0:d}  ", line.Date);
                foreach (var entry in line.Entries)
                {
                    Console.Write("{0} {1} ", new String(' ', 8), entry.Balance.ToString("N").PadRight(8));
                }

                Console.Write(line.BankBalance.ToString("N").PadRight(13));
                Console.Write(line.BankBalanceAdjustments.Sum(t => t.Credit - t.Debit).ToString("N").PadRight(12));
                Console.Write(line.BankBalances.Sum(b => b.Balance).ToString("N").PadRight(9));
                Console.WriteLine();

                if (outputTransactions)
                {
                    foreach (var entry in line.Entries)
                    {
                        foreach (var transaction in entry.Transactions)
                        {
                            Console.WriteLine("          {0} {1} {2} {3}",
                                entry.BucketCode.PadRight(6),
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
