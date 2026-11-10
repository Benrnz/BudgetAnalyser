namespace BudgetAnalyser.Engine.BankFeed;

/// <summary>
///     Thrown when a bank feed cannot be reached, returns an error, or returns data that cannot be used. When this is thrown during a download nothing is imported.
/// </summary>
public class BankFeedException : Exception
{
    public BankFeedException()
    {
    }

    public BankFeedException(string message) : base(message)
    {
    }

    public BankFeedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
