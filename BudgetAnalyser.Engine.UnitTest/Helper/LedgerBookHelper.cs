using System.Diagnostics;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;

namespace BudgetAnalyser.Engine.UnitTest.Helper;

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

    public static void Output(this LedgerBook book, bool outputTransactions = false, IReesTestOutput? outputWriter = null)
    {
        var writer = NonNullableOutputWriter(outputWriter);
        writer.WriteLine(string.Empty);
        writer.WriteLine("**************************** LEDGER BOOK OUTPUT *****************************");
        writer.WriteLine($"Name: {book.Name}");
        writer.WriteLine($"Filename: {book.StorageKey}");
        writer.WriteLine($"Modified: {book.Modified}");
        writer.Write("Date        ");
        var ledgerOrder = LedgerOrder(book);

        OutputReconciliationHeader(writer);

        foreach (var line in book.Reconciliations)
        {
            Output(line, ledgerOrder, outputTransactions, outputWriter: writer);
        }
    }

    private static IReesTestOutput NonNullableOutputWriter(IReesTestOutput? outputWriter)
    {
        return outputWriter ?? new DebugTestOutput();
    }

    public static void Output(this LedgerEntryLine line, IDictionary<BudgetBucket, int> ledgerOrder, bool outputTransactions = false, bool outputHeader = false, IReesTestOutput? outputWriter = null)
    {
        var writer = NonNullableOutputWriter(outputWriter);
        if (outputHeader)
        {
            OutputReconciliationHeader(writer);
        }

        writer.Write($"{line.Date:d}  ");
        // foreach (var entry in line.Entries.OrderBy(e => e.LedgerBucket.BudgetBucket))
        // {
        //     writer.Write($"{entry.NetAmount,-8:N} {entry.LedgerBucket.StoredInAccount.Name.Truncate(1)} {entry.Balance,-9:N}");
        // }

        writer.Write(line.CalculatedSurplus.ToString("N").PadRight(9));
        var balanceCount = 0;
        foreach (var bankBalance in line.BankBalances)
        {
            if (++balanceCount > 2)
            {
                // Only two bank balances are shown in the test output at this stage.
                break;
            }

            var balanceText = $"{bankBalance.Account.Name.Truncate(1)} {bankBalance.Balance:N} ";
            writer.Write(balanceText.PadLeft(13).TruncateLeft(13));
        }

        writer.Write(line.TotalBalanceAdjustments.ToString("N").PadLeft(9).TruncateLeft(9));
        writer.Write(line.LedgerBalance.ToString("N").PadLeft(13).TruncateLeft(13));
        writer.WriteLine(string.Empty);

        foreach (var entry in line.Entries.OrderBy(e => e.LedgerBucket.BudgetBucket))
        {
            var tab = "            ";
            writer.WriteLine($"{tab}{entry.LedgerBucket.BudgetBucket.Code,-7} Balance:{entry.Balance:N2}");
            if (outputTransactions)
            {
                foreach (var transaction in entry.Transactions)
                {
                    writer.WriteLine(
                        "{0}        {1:d} {2} {3} {4} {5}",
                        tab,
                        transaction.Date ?? line.Date,
                        transaction.Amount >= 0 ? (transaction.Amount.ToString("N") + "Cr").PadLeft(8) : (transaction.Amount.ToString("N") + "Dr").PadLeft(16),
                        transaction.Id,
                        transaction.AutoMatchingReference,
                        transaction.Narrative.Truncate(30));
                }
            }
        }

        writer.WriteLine("=================================================================================================================================");
    }

    public static void Output(this LedgerBookDto book, bool outputTransactions = false, IReesTestOutput? outputWriter = null)
    {
        var writer = NonNullableOutputWriter(outputWriter);
        writer.WriteLine("Name: {0}", book.Name);
        writer.WriteLine("Filename: {0}", book.StorageKey);
        writer.WriteLine("Modified: {0}", book.Modified);
        writer.Write("Date        ");
        foreach (var ledger in book.Reconciliations.SelectMany(l => l.Entries.Select(e => e.BucketCode)))
        {
            writer.Write(ledger.PadRight(18));
        }

        writer.Write("Surplus  BankBalance  Adjustments LedgerBalance");
        writer.WriteLine(string.Empty);
        writer.WriteLine("====================================================================================================================");

        foreach (var line in book.Reconciliations)
        {
            writer.Write($"{line.Date:d}  ");
            foreach (var entry in line.Entries)
            {
                writer.Write($"{new string(' ', 8)} {entry.Balance,-8:N} ");
            }

            writer.Write(line.BankBalance.ToString("N").PadRight(13));
            writer.Write(line.BankBalanceAdjustments.Sum(t => t.Amount).ToString("N").PadRight(12));
            writer.Write(line.BankBalances.Sum(b => b.Balance).ToString("N").PadRight(9));
            writer.WriteLine(string.Empty);

            if (outputTransactions)
            {
                foreach (var entry in line.Entries)
                {
                    foreach (var transaction in entry.Transactions)
                    {
                        writer.WriteLine(
                            "          {0} {1} {2}",
                            entry.BucketCode.PadRight(6),
                            transaction.Amount > 0 ? (transaction.Amount.ToString("N") + "Cr").PadLeft(8) : (transaction.Amount.ToString("N") + "Dr").PadLeft(16),
                            transaction.Narrative ?? string.Empty);
                    }
                }

                Debug.WriteLine("====================================================================================================================");
            }
        }
    }

    public static void Output(this LedgerEntry instance, IReesTestOutput? outputWriter = null)
    {
        var writer = NonNullableOutputWriter(outputWriter);
        writer.WriteLine("Ledger Entry Transactions. ============================================");
        foreach (var transaction in instance.Transactions)
        {
            writer.WriteLine($"{transaction.Date:d} {transaction.Narrative} {transaction.Amount:F2}");
        }

        writer.WriteLine("----------------------------------------------------------------------------------------");
        writer.WriteLine($"{instance.Transactions.Count()} transactions. NetAmount: {instance.NetAmount:F2} ClosingBalance: {instance.Balance:F2}");
    }

    private static void OutputReconciliationHeader(IReesTestOutput writer)
    {
        writer.WriteLine("Surplus    BankBalances    Adjusts AdjustBalance");
        writer.WriteLine("==============================================================================================================================================");
    }
}
