using BudgetAnalyser.Engine;
using CommunityToolkit.Mvvm.Messaging;

namespace BudgetAnalyser;

/// <summary>
///     A <see cref="IMessenger" /> that is thread safe when registering listeners, sending and unregistering.
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
public class ConcurrentMessenger : IMessenger
{
    private static readonly object SyncRoot = new();
    private readonly IMessenger defaultMessenger;
    private readonly ILogger logger;

    public ConcurrentMessenger([NotNull] IMessenger defaultMessenger, [NotNull] ILogger logger)
    {
        if (defaultMessenger is null)
        {
            throw new ArgumentNullException(nameof(defaultMessenger));
        }

        if (logger is null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        this.defaultMessenger = defaultMessenger;
        this.logger = logger;
    }

    public bool IsRegistered<TMessage, TToken>(object recipient, TToken token) where TMessage : class where TToken : IEquatable<TToken>
    {
        lock (SyncRoot)
        {
            return this.defaultMessenger.IsRegistered<TMessage, TToken>(recipient, token);
        }
    }

    public void Register<TRecipient, TMessage, TToken>(TRecipient recipient, TToken token, MessageHandler<TRecipient, TMessage> handler) where TRecipient : class where TMessage : class where TToken : IEquatable<TToken>
    {
        lock (SyncRoot)
        {
            if (this.defaultMessenger.IsRegistered<TMessage, TToken>(recipient, token)) return;
            this.defaultMessenger.Register(recipient, token, handler);
        }

        this.logger.LogInfo(l => l.Format("IMessenger.Register Token:{0} recipient:{1} Message:{2}", token, recipient, typeof(TMessage).Name));
        }

    public void UnregisterAll(object recipient)
    {
        lock (SyncRoot)
        {
            this.defaultMessenger.UnregisterAll(recipient);
        }
        
        this.logger.LogInfo(l => l.Format("IMessenger.UnregisterAll recipient:{0}", recipient));
    }

    public void UnregisterAll<TToken>(object recipient, TToken token) where TToken : IEquatable<TToken>
    {
        lock (SyncRoot)
        {
            this.defaultMessenger.UnregisterAll(recipient, token);
        }

        this.logger.LogInfo(l => l.Format("IMessenger.UnregisterAll Token:{0} recipient:{1}", token, recipient));
    }

    public void Unregister<TMessage, TToken>(object recipient, TToken token) where TMessage : class where TToken : IEquatable<TToken>
    {
        lock (SyncRoot)
        {
            if (!this.defaultMessenger.IsRegistered<TMessage, TToken>(recipient, token)) return;
            this.defaultMessenger.Unregister<TMessage, TToken>(recipient, token);
        }
    }

    public TMessage Send<TMessage, TToken>(TMessage message, TToken token) where TMessage : class where TToken : IEquatable<TToken>
    {
        TMessage result;
        lock (SyncRoot)
        {
            result = this.defaultMessenger.Send(message, token);
        }

        this.logger.LogInfo(l => l.Format("IMessenger.Send Token:{0} Message:{1} {2}", token, message, typeof(TMessage).Name));
        return result;
    }

    public void Cleanup()
    {
        lock (SyncRoot)
        {
            this.defaultMessenger.Cleanup();
        }

        this.logger.LogInfo(l => "IMessenger.Cleanup");
    }

    public void Reset()
    {
        lock (SyncRoot)
        {
            this.defaultMessenger.Reset();
        }

        this.logger.LogInfo(l => "IMessenger.Reset");
    }
}