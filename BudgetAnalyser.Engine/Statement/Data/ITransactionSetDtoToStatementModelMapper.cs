namespace BudgetAnalyser.Engine.Statement.Data
{
    public interface ITransactionSetDtoToStatementModelMapper
    {
        StatementModel Map(TransactionSetDto setDto);
    }
}