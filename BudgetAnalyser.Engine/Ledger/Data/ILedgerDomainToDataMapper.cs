namespace BudgetAnalyser.Engine.Ledger.Data
{
    public interface ILedgerDomainToDataMapper
    {
        DataLedgerBook Map(LedgerBook domainBook);
    }
}