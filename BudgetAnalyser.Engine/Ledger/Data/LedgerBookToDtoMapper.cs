namespace BudgetAnalyser.Engine.Ledger.Data
{
    [AutoRegisterWithIoC(SingleInstance = true, RegisterAs = typeof(BasicMapper<LedgerBook, LedgerBookDto>))]
    public class LedgerBookToDtoMapper : MagicMapper<LedgerBook, LedgerBookDto>
    {
    }
}