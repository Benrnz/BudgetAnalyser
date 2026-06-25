namespace BudgetAnalyser.Engine.Statement;

/// <summary>
///     An exception to represent an inconsistency in the <see cref="TransactionsListModel" /> loaded. The check sum does not
///     match the data.
/// </summary>
public class TransactionsListModelChecksumException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionsListModelChecksumException" /> class.
    /// </summary>
    public TransactionsListModelChecksumException(string message) : base(message)
    {
    }
}
