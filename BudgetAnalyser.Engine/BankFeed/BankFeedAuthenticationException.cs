namespace BudgetAnalyser.Engine.BankFeed;

/// <summary>
///     Thrown when bank feed credentials are missing, or have been rejected by the bank feed (for example because they were regenerated or revoked). The user needs to re-enter their credentials.
/// </summary>
public class BankFeedAuthenticationException : BankFeedException
{
    public BankFeedAuthenticationException()
    {
    }

    public BankFeedAuthenticationException(string message) : base(message)
    {
    }

    public BankFeedAuthenticationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
