namespace BudgetAnalyser.Engine.Statement.Data
{
    public interface IStatementModelToTransactionSetDtoMapper
    {
        TransactionSetDto Map(StatementModel model, string versionHash, string fileName);
    }
}