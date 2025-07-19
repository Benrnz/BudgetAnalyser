namespace BudgetAnalyser.Engine.Statement;

/// <summary>
///     An exception to represent an inconsistency in the <see cref="TransactionSetModel" /> loaded. The check sum does not
///     match the data.
/// </summary>
public class TransactionsModelChecksumException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionsModelChecksumException" /> class.
    /// </summary>
    public TransactionsModelChecksumException(string message) : base(message)
    {
    }
}
