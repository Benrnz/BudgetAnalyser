namespace BudgetAnalyser.Engine.Services;

/// <summary>
///     An exception that describes an attempt to open an encrypted file with an incorrect set of credentials.
/// </summary>
/// <seealso cref="System.Exception" />
public class EncryptionKeyIncorrectException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EncryptionKeyIncorrectException" /> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public EncryptionKeyIncorrectException(string message)
        : base(message)
    {
    }
}
