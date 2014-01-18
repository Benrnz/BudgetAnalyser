using System;

namespace BudgetAnalyser.Engine.Ledger
{
    public class DataLedgerTransaction
    {
        public Guid Id { get; set; }

        public decimal Credit { get; set; }

        public decimal Debit { get; set; }

        public string Narrative { get; set; }

        public string TransactionType { get; set; }
    }
}