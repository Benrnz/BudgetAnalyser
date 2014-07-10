using System;

namespace BudgetAnalyser.Engine.Statement.Data
{
    public class TransactionDto
    {
        public string AccountType { get; set; }

        public decimal Amount { get; set; }

        public string BudgetBucketCode { get; set; }

        public DateTime Date { get; set; }

        public string Description { get; set; }

        /// <summary>
        ///     The unique identifier for the transaction.  Ideally this should not be public settable, but this is used during
        ///     serialisation.
        /// </summary>
        public Guid Id { get; set; }

        public string Reference1 { get; set; }

        public string Reference2 { get; set; }

        public string Reference3 { get; set; }

        public string TransactionType { get; set; }
    }
}