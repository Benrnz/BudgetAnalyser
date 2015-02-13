using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.Engine.Ledger
{
    public class LastLedgerBookLoadedV1 : IPersistent
    {
        public string LedgerBookStorageKey { get; set; }

        public int LoadSequence { get { return 50; } }
    }
}