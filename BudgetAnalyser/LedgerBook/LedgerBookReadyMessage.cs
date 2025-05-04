using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook;

public class LedgerBookReadyMessage : MessageBase
{
    public LedgerBookReadyMessage(Engine.Ledger.LedgerBook? ledgerBook)
    {
        LedgerBook = ledgerBook;
    }

    public bool ForceUiRefresh { get; set; }

    /// <summary>
    ///     The new Ledger Book.  This may be null if the Ledger Book is closed.
    /// </summary>
    public Engine.Ledger.LedgerBook? LedgerBook { get; private set; }
}
