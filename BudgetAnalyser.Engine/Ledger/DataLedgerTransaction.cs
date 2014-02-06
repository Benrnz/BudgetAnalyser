using System;

namespace BudgetAnalyser.Engine.Ledger
{
    public class DataLedgerTransaction
    {
        public decimal Credit { get; set; }

        public decimal Debit { get; set; }
        public Guid Id { get; set; }

        public string Narrative { get; set; }

        public string TransactionType { get; set; }
    }
}