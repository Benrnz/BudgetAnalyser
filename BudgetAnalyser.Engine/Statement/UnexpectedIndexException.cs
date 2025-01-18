namespace BudgetAnalyser.Engine.Statement;

/// <summary>
///     An exception to represent an index out of range exception when parsing a Bank Statement CSV file.
/// </summary>
/// <seealso cref="System.Exception" />
public class UnexpectedIndexException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UnexpectedIndexException" /> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public UnexpectedIndexException(string message) : base(message)
    {
    }
}
