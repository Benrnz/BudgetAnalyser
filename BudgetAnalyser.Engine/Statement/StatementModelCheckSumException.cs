namespace BudgetAnalyser.Engine.Statement;

/// <summary>
///     An exception to represent an inconsistency in the <see cref="TransactionSetModel" /> loaded. The check sum does not
///     match the data.
/// </summary>
public class StatementModelChecksumException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StatementModelChecksumException" /> class.
    /// </summary>
    public StatementModelChecksumException(string message) : base(message)
    {
    }
}
