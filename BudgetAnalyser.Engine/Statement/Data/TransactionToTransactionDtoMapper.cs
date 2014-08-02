namespace BudgetAnalyser.Engine.Statement.Data
{
    [AutoRegisterWithIoC(RegisterAs = typeof(BasicMapper<Transaction, TransactionDto>))]
    public class TransactionToTransactionDtoMapper : MagicMapper<Transaction, TransactionDto>
    {
    }
}