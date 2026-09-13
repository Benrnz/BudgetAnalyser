namespace BudgetAnalyser.Engine.Persistence;

/// <summary>
///     An exception used when a bad Application State file is read that is not compatible with this application. Might
///     indicate tampering, or an old now unsupported file version.
/// </summary>
public class BadApplicationStateFileFormatException : IOException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BadApplicationStateFileFormatException" /> class.
    /// </summary>
    public BadApplicationStateFileFormatException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BadApplicationStateFileFormatException" /> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public BadApplicationStateFileFormatException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BadApplicationStateFileFormatException" /> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public BadApplicationStateFileFormatException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
