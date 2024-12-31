using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Ledger.Data;

/// <summary>
///     A Dto for <see cref="BankBalance" />
/// </summary>
[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "DataBank", Justification = "Not intended to mean Databank.")]
public class BankBalanceDto
{
    /// <summary>
    ///     Gets or sets the account.
    /// </summary>
    public string Account { get; set; }

    /// <summary>
    ///     Gets or sets the balance.
    /// </summary>
    public decimal Balance { get; set; }
}
