namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     An exception to report data corruption issues in the LedgerBook data.
    /// </summary>
    public class CorruptedLedgerBookException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CorruptedLedgerBookException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public CorruptedLedgerBookException(string message)
            : base(message)
        {
        }
    }
}
