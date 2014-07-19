namespace BudgetAnalyser.Engine.Ledger.Data
{
    public interface ILedgerDataToDomainMapper
    {
        LedgerBook Map(LedgerBookDto dataBook);
    }
}