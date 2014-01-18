namespace BudgetAnalyser.Engine.Ledger
{
    public interface ILedgerDataToDomainMapper
    {
        LedgerBook Map(DataLedgerBook dataBook);
    }
}