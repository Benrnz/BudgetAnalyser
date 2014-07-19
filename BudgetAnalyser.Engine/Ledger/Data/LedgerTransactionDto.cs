using System;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    public class LedgerTransactionDto
    {
        public string AccountType { get; set; }

        public decimal Credit { get; set; }

        public decimal Debit { get; set; }
        public Guid Id { get; set; }

        public string Narrative { get; set; }

        public string TransactionType { get; set; }
    }
}