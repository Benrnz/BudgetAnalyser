using BudgetAnalyser.Engine;
using CommunityToolkit.Mvvm.Messaging;

namespace BudgetAnalyser;

/// <summary>
///     A Galasoft <see cref="IMessenger" /> that is thread safe when registering listeners and unregistering.
///     No locking occurs when sending.
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
public class ConcurrentMessenger : IMessenger
{
    private static readonly object SyncRoot = new();
    private readonly IMessenger defaultMessenger;
    private readonly ILogger logger;

    public ConcurrentMessenger([NotNull] IMessenger defaultMessenger, [NotNull] ILogger logger)
    {
        if (defaultMessenger == null)
        {
            throw new ArgumentNullException(nameof(defaultMessenger));
        }

        if (logger == null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        this.defaultMessenger = defaultMessenger;
        this.logger = logger;
    }

    public void Register<TMessage>(object recipient, Action<TMessage> action, bool keepTargetAlive = false)
    {
        lock (SyncRoot)
        {
            this.defaultMessenger.Register(recipient, action);
        }

        this.logger.LogInfo(l => l.Format("IMessenger.Register {0} for Message: {1}", recipient, typeof(TMessage).Name));
    }

    public void Register<TMessage>(object recipient, object token, Action<TMessage> action, bool keepTargetAlive = false)
    {
        Register(recipient, action, keepTargetAlive);
    }

    public void Register<TMessage>(object recipient, object token, bool receiveDerivedMessagesToo, Action<TMessage> action, bool keepTargetAlive = false)
    {
        Register(recipient, action, keepTargetAlive);
    }

    public void Register<TMessage>(object recipient, bool receiveDerivedMessagesToo, Action<TMessage> action, bool keepTargetAlive = false)
    {
        Register(recipient, action, keepTargetAlive);
    }

    public void Send<TMessage>(TMessage message)
    {
        lock (SyncRoot)
        {
            this.defaultMessenger.Send(message);
        }

        this.logger.LogInfo(l => l.Format("IMessenger.Send {0}", message));
    }

    public void Send<TMessage, TTarget>(TMessage message)
    {
        lock (SyncRoot)
        {
            this.defaultMessenger.Send<TMessage, TTarget>(message);
        }

        this.logger.LogInfo(l => l.Format("IMessenger.Send {0} to target {1}", message, typeof(TTarget).FullName));
    }

    public void Send<TMessage>(TMessage message, object token)
    {
        lock (SyncRoot)
        {
            this.defaultMessenger.Send(message, token);
        }

        this.logger.LogInfo(l => l.Format("IMessenger.Send {0} with token {1}", message, token));
    }

    public void Unregister(object recipient)
    {
        lock (SyncRoot)
        {
            this.defaultMessenger.Unregister(recipient);
        }

        this.logger.LogInfo(l => l.Format("IMessenger.Unregister {0}", recipient));
    }

    public void Unregister<TMessage>(object recipient)
    {
        lock (SyncRoot)
        {
            this.defaultMessenger.Unregister(recipient);
        }

        this.logger.LogInfo(l => l.Format("IMessenger.Unregister {0}", recipient));
    }

    public void Unregister<TMessage>(object recipient, object token)
    {
        lock (SyncRoot)
        {
            this.defaultMessenger.Unregister<TMessage>(recipient, token);
        }

        this.logger.LogInfo(l => l.Format("IMessenger.Unregister {0} with token {1}", recipient, token));
    }

    public void Unregister<TMessage>(object recipient, Action<TMessage> action)
    {
        lock (SyncRoot)
        {
            this.defaultMessenger.Unregister(recipient, action);
        }

        this.logger.LogInfo(l => l.Format("IMessenger.Unregister {0}", recipient));
    }

    public void Unregister<TMessage>(object recipient, object token, Action<TMessage> action)
    {
        lock (SyncRoot)
        {
            this.defaultMessenger.Unregister(recipient, token, action);
        }

        this.logger.LogInfo(l => l.Format("IMessenger.Unregister {0} with token {1}", recipient, token));
    }

}