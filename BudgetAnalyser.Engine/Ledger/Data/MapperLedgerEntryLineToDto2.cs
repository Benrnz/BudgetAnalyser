using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Ledger.Data;

internal class MapperLedgerEntryLineToDto2(
    IAccountTypeRepository accountTypeRepository,
    ILedgerBucketFactory ledgerBucketFactory,
    ILedgerTransactionFactory ledgerTransactionFactory) : IDtoMapper<LedgerEntryLineDto, LedgerEntryLine>
{
    private readonly IDtoMapper<LedgerTransactionDto, BankBalanceAdjustmentTransaction> bankAdjustmentMapper = new MapperBankBalanceAdjustmentToDto2(accountTypeRepository);
    private readonly IDtoMapper<BankBalanceDto, BankBalance> bankBalanceMapper = new MapperBankBalanceToDto2(accountTypeRepository);
    private readonly IDtoMapper<LedgerEntryDto, LedgerEntry> ledgerEntriesMapper = new MapperLedgerEntryToDto2(ledgerBucketFactory, ledgerTransactionFactory);

    public LedgerEntryLineDto ToDto(LedgerEntryLine model)
    {
        var dto = new LedgerEntryLineDto
        (
            model.TotalBankBalance,
            model.BankBalanceAdjustments.Select(this.bankAdjustmentMapper.ToDto).ToArray(),
            model.BankBalances.Select(this.bankBalanceMapper.ToDto).ToArray(),
            model.Date,
            Remarks: model.Remarks,
            Entries: model.Entries.Select(this.ledgerEntriesMapper.ToDto).ToArray()
        );

        return dto;
    }

    public LedgerEntryLine ToModel(LedgerEntryLineDto dto)
    {
        var bankBalanceAdjustments = dto.BankBalanceAdjustments.Select(this.bankAdjustmentMapper.ToModel);
        var bankBalances = dto.BankBalances.Select(this.bankBalanceMapper.ToModel);
        var entries = dto.Entries.Select(this.ledgerEntriesMapper.ToModel);

        var line = new LedgerEntryLine(bankBalanceAdjustments, bankBalances, entries) { Date = dto.Date, Remarks = dto.Remarks };

        return line;
    }
}
