using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;

namespace BudgetAnalyser.Engine.UnitTest.Helper
{
    public static class LedgerBookHelper
    {
        public static Dictionary<BudgetBucket, int> LedgerOrder(LedgerBook book)
        {
            var ledgerOrder = new Dictionary<BudgetBucket, int>();
            var index = 0;
            foreach (var ledger in book.Ledgers.OrderBy(l => l.BudgetBucket))
            {
                Debug.Write(ledger.BudgetBucket.Code.PadRight(20));
                ledgerOrder.Add(ledger.BudgetBucket, index++);
            }
            return ledgerOrder;
        }

        public static void Output(this LedgerBook book, bool outputTransactions = false)
        {
            Debug.WriteLine($"Name: {book.Name}");
            Debug.WriteLine($"Filename: {book.StorageKey}");
            Debug.WriteLine($"Modified: {book.Modified}");
            Debug.Write("Date        ");
            Dictionary<BudgetBucket, int> ledgerOrder = LedgerOrder(book);

            OutputReconciliationHeader();

            foreach (var line in book.Reconciliations)
            {
                Output(line, ledgerOrder, outputTransactions);
            }
        }

        public static void Output(this LedgerEntryLine line, Dictionary<BudgetBucket, int> ledgerOrder, bool outputTransactions = false, bool outputHeader = false)
        {
            if (outputHeader)
            {
                OutputReconciliationHeader();
            }

            Debug.Write($"{line.Date:d}  ");
            foreach (var entry in line.Entries.OrderBy(e => e.LedgerBucket.BudgetBucket))
            {
                Debug.Write($"{entry.NetAmount.ToString("N").PadRight(8)} {entry.LedgerBucket.StoredInAccount.Name.Truncate(1)} {entry.Balance.ToString("N").PadRight(9)}");
            }

            Debug.Write(line.CalculatedSurplus.ToString("N").PadRight(9));
            var balanceCount = 0;
            foreach (var bankBalance in line.BankBalances)
            {
                if (++balanceCount > 2)
                {
                    break;
                }
                // Only two bank balances are shown in the test output at this stage.
                var balanceText = string.Format("{0} {1} ", bankBalance.Account.Name.Truncate(1), bankBalance.Balance.ToString("N"));
                Debug.Write(balanceText.PadLeft(13).TruncateLeft(13));
            }
            Debug.Write(line.TotalBalanceAdjustments.ToString("N").PadLeft(9).TruncateLeft(9));
            Debug.Write(line.LedgerBalance.ToString("N").PadLeft(13).TruncateLeft(13));
            Debug.WriteLine(string.Empty);

            if (outputTransactions)
            {
                foreach (var entry in line.Entries.OrderBy(e => e.LedgerBucket.BudgetBucket))
                {
                    var tab = new string(' ', 11 + 18 * ledgerOrder[entry.LedgerBucket.BudgetBucket]);
                    foreach (var transaction in entry.Transactions)
                    {
                        Debug.WriteLine(
                                        "{0} {1} {2} {3} {4} {5}",
                                        tab,
                                        entry.LedgerBucket.BudgetBucket.Code.PadRight(6),
                                        transaction.Amount >= 0 ? (transaction.Amount.ToString("N") + "Cr").PadLeft(8) : (transaction.Amount.ToString("N") + "Dr").PadLeft(16),
                                        transaction.Narrative.Truncate(15),
                                        transaction.Id,
                                        transaction.AutoMatchingReference);
                    }
                }
                Debug.WriteLine("=================================================================================================================================");
            }
        }

        public static void Output(this LedgerBookDto book, bool outputTransactions = false)
        {
            Debug.WriteLine("Name: {0}", book.Name);
            Debug.WriteLine("Filename: {0}", book.StorageKey);
            Debug.WriteLine("Modified: {0}", book.Modified);
            Debug.Write("Date        ");
            foreach (var ledger in book.Reconciliations.SelectMany(l => l.Entries.Select(e => e.BucketCode)))
            {
                Debug.Write(ledger.PadRight(18));
            }

            Debug.Write("Surplus  BankBalance  Adjustments LedgerBalance");
            Debug.WriteLine(string.Empty);
            Debug.WriteLine("====================================================================================================================");

            foreach (var line in book.Reconciliations)
            {
                Debug.Write($"{line.Date:d}  ");
                foreach (var entry in line.Entries)
                {
                    Debug.Write($"{new string(' ', 8)} {entry.Balance.ToString("N").PadRight(8)} ");
                }

                Debug.Write(line.BankBalance.ToString("N").PadRight(13));
                Debug.Write(line.BankBalanceAdjustments.Sum(t => t.Amount).ToString("N").PadRight(12));
                Debug.Write(line.BankBalances.Sum(b => b.Balance).ToString("N").PadRight(9));
                Debug.WriteLine(string.Empty);

                if (outputTransactions)
                {
                    foreach (var entry in line.Entries)
                    foreach (var transaction in entry.Transactions)
                    {
                        Debug.WriteLine(
                                        "          {0} {1} {2}",
                                        entry.BucketCode.PadRight(6),
                                        transaction.Amount > 0 ? (transaction.Amount.ToString("N") + "Cr").PadLeft(8) : (transaction.Amount.ToString("N") + "Dr").PadLeft(16),
                                        transaction.Narrative);
                    }
                    Debug.WriteLine("====================================================================================================================");
                }
            }
        }

        public static void Output(this LedgerEntry instance)
        {
            Debug.WriteLine($"Ledger Entry Transactions. ============================================");
            foreach (var transaction in instance.Transactions)
            {
                Debug.WriteLine($"{transaction.Date:d} {transaction.Narrative} {transaction.Amount:F2}");
            }
            Debug.WriteLine("----------------------------------------------------------------------------------------");
            Debug.WriteLine($"{instance.Transactions.Count()} transactions. NetAmount: {instance.NetAmount:F2} ClosingBalance: {instance.Balance:F2}");
        }

        private static void OutputReconciliationHeader()
        {
            Debug.Write("Surplus    BankBalances    Adjusts LedgerBalance");
            Debug.WriteLine(string.Empty);
            Debug.WriteLine("==============================================================================================================================================");
        }
    }
}