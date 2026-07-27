namespace BudgetAnalyser.Engine.Ledger.Data;

/// <summary>
///     A Dto for <see cref="LedgerEntryLine" />
/// </summary>
public record LedgerEntryLineDto(decimal BankBalance, LedgerTransactionDto[] BankBalanceAdjustments, BankBalanceDto[] BankBalances, DateOnly Date, LedgerEntryDto[] Entries, string Remarks);
