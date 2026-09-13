namespace Rees.Wpf;

public class MessageBase
{
    /// <summary>
    ///     Initializes a new instance of the MessageBase class.
    /// </summary>
    protected MessageBase()
    {
    }

    /// <summary>
    ///     Gets or sets the message's sender.
    /// </summary>
    public object? Sender
    {
        get;
        init;
    }
}
