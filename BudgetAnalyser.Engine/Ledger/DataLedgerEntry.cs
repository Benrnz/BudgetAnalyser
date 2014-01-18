using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Ledger
{
    public class DataLedgerEntry
    {
        public DataLedgerEntry()
        {
            Transactions = new List<DataLedgerTransaction>();
        }

        public string BucketCode { get; set; }

        public List<DataLedgerTransaction> Transactions { get; set; }

        public decimal Balance { get; set; }
    }
}