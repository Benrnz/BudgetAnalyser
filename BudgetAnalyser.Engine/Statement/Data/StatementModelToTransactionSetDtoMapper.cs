namespace BudgetAnalyser.Engine.Statement.Data
{
    [AutoRegisterWithIoC(SingleInstance = true, RegisterAs = typeof(BasicMapper<StatementModel, TransactionSetDto>))]
    public class StatementModelToTransactionSetDtoMapper : MagicMapper<StatementModel, TransactionSetDto>
    {
    }
}