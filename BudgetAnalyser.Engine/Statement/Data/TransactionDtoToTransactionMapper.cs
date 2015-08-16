namespace BudgetAnalyser.Engine.Statement.Data
{
    [AutoRegisterWithIoC(RegisterAs = typeof(BasicMapper<TransactionDto, Transaction>))]
    public class TransactionDtoToTransactionMapper : MagicMapper<TransactionDto, Transaction>
    {
    }
}