namespace BudgetAnalyser.Engine.Statement.Data
{
    [AutoRegisterWithIoC(RegisterAs = typeof(BasicMapper<TransactionSetDto, StatementModel>))]
    public class TransactionSetDtoToStatementModelMapper : MagicMapper<TransactionSetDto, StatementModel>
    {
    }
}