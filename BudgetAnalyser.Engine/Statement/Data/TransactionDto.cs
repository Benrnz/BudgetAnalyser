using System;

namespace BudgetAnalyser.Engine.Statement.Data
{
    /// <summary>
    ///     A Dto to persist a single transaction from a statement.
    /// </summary>
    public class TransactionDto
    {
        /// <summary>
        ///     Gets or sets the account code.
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        ///     Gets or sets the transaction amount, debits are negative.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        ///     Gets or sets the budget bucket code.
        /// </summary>
        public string BudgetBucketCode { get; set; }

        /// <summary>
        ///     Gets or sets the transaction date.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        ///     Gets or sets the transaction description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     The unique identifier for the transaction.  Ideally this should not be public settable, but this is used during
        ///     serialisation.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        ///     Gets or sets the transaction reference1.
        /// </summary>
        public string Reference1 { get; set; }

        /// <summary>
        ///     Gets or sets the transaction reference2.
        /// </summary>
        public string Reference2 { get; set; }

        /// <summary>
        ///     Gets or sets the transaction reference3.
        /// </summary>
        public string Reference3 { get; set; }

        /// <summary>
        ///     Gets or sets the type code of the transaction.
        /// </summary>
        public string TransactionType { get; set; }
    }
}
