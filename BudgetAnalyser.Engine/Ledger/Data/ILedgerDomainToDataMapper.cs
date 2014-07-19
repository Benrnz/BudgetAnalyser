namespace BudgetAnalyser.Engine.Ledger.Data
{
    public interface ILedgerDomainToDataMapper
    {
        LedgerBookDto Map(LedgerBook domainBook);
    }
}