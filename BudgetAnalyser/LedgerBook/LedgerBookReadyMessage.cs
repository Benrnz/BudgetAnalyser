using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook;

public class LedgerBookReadyMessage : MessageBase
{
    public LedgerBookReadyMessage(Engine.Ledger.LedgerBook? ledgerBook)
    {
        LedgerBook = ledgerBook;
    }

    public bool ForceUiRefresh { get; set; }
    public Engine.Ledger.LedgerBook? LedgerBook { get; private set; }
}
