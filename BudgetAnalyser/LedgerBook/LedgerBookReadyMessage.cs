using GalaSoft.MvvmLight.Messaging;

namespace BudgetAnalyser.LedgerBook
{
    public class LedgerBookReadyMessage : MessageBase
    {
        public LedgerBookReadyMessage(Engine.Ledger.LedgerBook ledgerBook)
        {
            LedgerBook = ledgerBook;
        }

        public Engine.Ledger.LedgerBook LedgerBook { get; private set; }

        public bool ForceUiRefresh { get; set; }
    }
}