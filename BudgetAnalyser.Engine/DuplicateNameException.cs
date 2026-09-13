namespace BudgetAnalyser.Engine;

/// <summary>
///     An exception that describes a problem with duplicate data , where no duplicates are expected.
/// </summary>
public class DuplicateNameException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DuplicateNameException" /> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public DuplicateNameException(string message) : base(message)
    {
    }
}
