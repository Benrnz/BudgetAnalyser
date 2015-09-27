namespace BudgetAnalyser.Engine.Ledger
{
    public class TransferFundsCommand
    {
        public string AutoMatchingReference { get; set; }

        public bool BankTransferRequired { get; set; }

        public LedgerBucket FromLedger { get; set; }

        public string Narrative { get; set; }

        public LedgerBucket ToLedger { get; set; }
        public decimal TransferAmount { get; set; }
    }
}