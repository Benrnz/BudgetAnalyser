namespace BudgetAnalyser.Engine.Ledger.Data;

/// <summary>
///     A Dto for <see cref="LedgerEntryLine" />
/// </summary>
public class LedgerEntryLineDto
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LedgerEntryLineDto" /> class.
    /// </summary>
    public LedgerEntryLineDto()
    {
        Entries = new List<LedgerEntryDto>();
        BankBalanceAdjustments = new List<LedgerTransactionDto>();
        BankBalances = new List<BankBalanceDto>();
    }

    /// <summary>
    ///     Total bank balance, ie sum of all <see cref="BankBalances" />
    ///     Doesn't include balance adjustments.
    /// </summary>
    public decimal BankBalance { get; set; }

    /// <summary>
    ///     Gets or sets the bank balance adjustments.
    /// </summary>
    public List<LedgerTransactionDto> BankBalanceAdjustments { get; init; }

    /// <summary>
    ///     Gets or sets the bank balances.
    /// </summary>
    public List<BankBalanceDto> BankBalances { get; init; }

    /// <summary>
    ///     Gets or sets the date of the reconciliation.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    ///     Gets or sets the entries.
    /// </summary>
    public List<LedgerEntryDto> Entries { get; init; }

    /// <summary>
    ///     Gets or sets the remarks.
    /// </summary>
    public string Remarks { get; init; } = string.Empty;
}
