namespace BudgetAnalyser.Engine.Ledger
{
    public interface ILedgerDomainToDataMapper
    {
        DataLedgerBook Map(LedgerBook domainBook);
    }
}