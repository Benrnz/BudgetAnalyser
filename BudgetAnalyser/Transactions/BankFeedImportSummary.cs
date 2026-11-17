using System.Globalization;
using System.Text;
using BudgetAnalyser.Engine.BankFeed;

namespace BudgetAnalyser.Transactions;

/// <summary>
///     Describes the outcome of a bank feed download for the result message box.
/// </summary>
public static class BankFeedImportSummary
{
    public static string Describe(BankFeedImportResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        var alreadyImported = result.Accounts.Sum(a => a.AlreadyImported);
        var text = new StringBuilder();
        if (result.TotalImported == 0)
        {
            text.Append("No new transactions in that date range.");
        }
        else
        {
            var perAccount = string.Join(", ", result.Accounts.Where(a => a.Imported > 0).Select(a => $"{a.Account.Name} {a.Imported}"));
            text.Append(string.Format(
                CultureInfo.CurrentCulture,
                "Imported {0} transaction{1} ({2:d MMM} – {3:d MMM}): {4}.",
                result.TotalImported,
                result.TotalImported == 1 ? string.Empty : "s",
                result.Range.Start,
                result.Range.End,
                perAccount));
            if (result.SuspectedDuplicates > 0)
            {
                text.Append(string.Format(CultureInfo.CurrentCulture, " {0} look like duplicates and have been flagged for review.", result.SuspectedDuplicates));
            }
        }

        if (alreadyImported > 0)
        {
            text.Append(string.Format(CultureInfo.CurrentCulture, " {0} had already been imported and were skipped.", alreadyImported));
        }

        return text.ToString();
    }
}
