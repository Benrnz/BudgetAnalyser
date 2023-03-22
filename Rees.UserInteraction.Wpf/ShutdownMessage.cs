using GalaSoft.MvvmLight.Messaging;

namespace Rees.Wpf
{
    /// <summary>
    /// A message object for use with the <see cref="IMessenger"/>. This message signifies the application is shutting down. This gives components a chance to perform actions prior to shutdown.
    /// </summary>
    public class ShutdownMessage : MessageBase
    {
    }
}