using System;
using System.Globalization;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using GalaSoft.MvvmLight.Messaging;

namespace BudgetAnalyser
{
    /// <summary>
    ///     A Galasoft <see cref="IMessenger" /> that is thread safe when registering listeners and unregistering.
    ///     No locking occurs when sending.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class ConcurrentMessenger : IMessenger
    {
        private static readonly object SyncRoot = new object();
        private readonly IMessenger defaultMessenger;
        private readonly ILogger logger;

        public ConcurrentMessenger([NotNull] IMessenger defaultMessenger, [NotNull] ILogger logger)
        {
            if (defaultMessenger == null)
            {
                throw new ArgumentNullException("defaultMessenger");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.defaultMessenger = defaultMessenger;
            this.logger = logger;
        }

        public void Register<TMessage>(object recipient, Action<TMessage> action)
        {
            lock (SyncRoot)
            {
                this.defaultMessenger.Register(recipient, action);
            }

            this.logger.LogInfo(() => string.Format(CultureInfo.CurrentCulture, "IMessenger.Register {0} for Message: {1}", recipient, typeof(TMessage).Name));
        }

        public void Register<TMessage>(object recipient, object token, Action<TMessage> action)
        {
            lock (SyncRoot)
            {
                this.defaultMessenger.Register(recipient, token, action);
            }

            this.logger.LogInfo(() => string.Format("IMessenger.Register {0} with token {1} for Message: {2}", recipient, token, typeof(TMessage).Name));
        }

        public void Register<TMessage>(object recipient, object token, bool receiveDerivedMessagesToo, Action<TMessage> action)
        {
            lock (SyncRoot)
            {
                this.defaultMessenger.Register(recipient, token, receiveDerivedMessagesToo, action);
            }

            this.logger.LogInfo(() => string.Format("IMessenger.Register {0} with token {1} for Message: {2} include derived messages.", recipient, token, typeof(TMessage).Name));
        }

        public void Register<TMessage>(object recipient, bool receiveDerivedMessagesToo, Action<TMessage> action)
        {
            lock (SyncRoot)
            {
                this.defaultMessenger.Register(recipient, receiveDerivedMessagesToo, action);
            }

            this.logger.LogInfo(() => string.Format("IMessenger.Register {0} for Message {1} include derived messages.", recipient, typeof(TMessage).Name));
        }

        public void Send<TMessage>(TMessage message)
        {
            this.defaultMessenger.Send(message);
            this.logger.LogInfo(() => string.Format("IMessenger.Send {0}", message));
        }

        public void Send<TMessage, TTarget>(TMessage message)
        {
            this.defaultMessenger.Send<TMessage, TTarget>(message);
            this.logger.LogInfo(() => string.Format("IMessenger.Send {0} to target {1}", message, typeof(TTarget).FullName));
        }

        public void Send<TMessage>(TMessage message, object token)
        {
            this.defaultMessenger.Send(message, token);
            this.logger.LogInfo(() => string.Format("IMessenger.Send {0} with token {1}", message, token));
        }

        public void Unregister(object recipient)
        {
            lock (SyncRoot)
            {
                this.defaultMessenger.Unregister(recipient);
            }

            this.logger.LogInfo(() => string.Format("IMessenger.Unregister {0}", recipient));
        }

        public void Unregister<TMessage>(object recipient)
        {
            lock (SyncRoot)
            {
                this.defaultMessenger.Unregister(recipient);
            }

            this.logger.LogInfo(() => string.Format("IMessenger.Unregister {0}", recipient));
        }

        public void Unregister<TMessage>(object recipient, object token)
        {
            lock (SyncRoot)
            {
                this.defaultMessenger.Unregister<TMessage>(recipient, token);
            }

            this.logger.LogInfo(() => string.Format("IMessenger.Unregister {0} with token {1}", recipient, token));
        }

        public void Unregister<TMessage>(object recipient, Action<TMessage> action)
        {
            lock (SyncRoot)
            {
                this.defaultMessenger.Unregister(recipient, action);
            }

            this.logger.LogInfo(() => string.Format("IMessenger.Unregister {0}", recipient));
        }

        public void Unregister<TMessage>(object recipient, object token, Action<TMessage> action)
        {
            lock (SyncRoot)
            {
                this.defaultMessenger.Unregister(recipient, token, action);
            }

            this.logger.LogInfo(() => string.Format("IMessenger.Unregister {0} with token {1}", recipient, token));
        }
    }
}