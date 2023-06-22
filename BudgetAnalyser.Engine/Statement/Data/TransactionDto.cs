using System;

namespace BudgetAnalyser.Engine.Statement.Data
{
    /// <summary>
    ///     A Dto to persist a single transaction from a statement.
    /// </summary>
    public class TransactionDto
    {
        private string budgetBucketCode;
        private string account;
        private string description;
        private string reference1;
        private string reference2;
        private string reference3;
        private string transactionType;

        /// <summary>
        ///     Gets or sets the account code.
        /// </summary>
        public string Account { get => account; set => account = CleanString(value); }

        /// <summary>
        ///     Gets or sets the transaction amount, debits are negative.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        ///     Gets or sets the budget bucket code.
        /// </summary>
        public string BudgetBucketCode { get => budgetBucketCode; set => budgetBucketCode = CleanString(value); }

        /// <summary>
        ///     Gets or sets the transaction date.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        ///     Gets or sets the transaction description.
        /// </summary>
        public string Description { get => description; set => description = CleanString(value); }

        /// <summary>
        ///     The unique identifier for the transaction.  Ideally this should not be public settable, but this is used during
        ///     serialisation.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        ///     Gets or sets the transaction reference1.
        /// </summary>
        public string Reference1 { get => reference1; set => reference1 = CleanString(value); }

        /// <summary>
        ///     Gets or sets the transaction reference2.
        /// </summary>
        public string Reference2 { get => reference2; set => reference2 = CleanString(value); }

        /// <summary>
        ///     Gets or sets the transaction reference3.
        /// </summary>
        public string Reference3 { get => reference3; set => reference3 = CleanString(value); }

        /// <summary>
        ///     Gets or sets the type code of the transaction.
        /// </summary>
        public string TransactionType { get => transactionType; set => transactionType = CleanString(value); }

        private string CleanString(string data)
        {
            return data?.Replace(",", string.Empty);
        }
    }
}