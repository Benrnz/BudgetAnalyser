namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     A To Do Task that represents a need to manually perform a transfer in order to balance the ledger buckets.
    /// </summary>
    public class TransferTask : ToDoTask
    {
        public TransferTask(string description, bool systemGenerated = false, bool canDelete = true) : base(description, systemGenerated, canDelete)
        {
        }

        /// <summary>
        ///     The amount of the transfer. This number should not be negative, there are no debits or credits in this context.
        /// </summary>
        public decimal Amount { get; internal set; }

        /// <summary>
        ///     The <see cref="LedgerBucket" /> that requires the transfer.
        /// </summary>
        public string BucketCode { get; internal set; }

        /// <summary>
        ///     Funds need to be transfered into this account. This is where the <see cref="LedgerBucket" /> is stored.
        /// </summary>
        public BankAccount.Account DestinationAccount { get; internal set; }

        /// <summary>
        ///     The auto-matching reference.  IMPORTANT: Only use this if you wish a single use matching rule to be automatically
        ///     created to match the transaction when its imported in a bank statement.
        /// </summary>
        public string Reference { get; internal set; }

        /// <summary>
        ///     Funds need to be transfered out of this account. Typically this is the Salary account.
        /// </summary>
        public BankAccount.Account SourceAccount { get; internal set; }
    }
}