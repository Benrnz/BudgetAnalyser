using System;
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

        public ConcurrentMessenger([NotNull] IMessenger defaultMessenger)
        {
            if (defaultMessenger == null)
            {
                throw new ArgumentNullException("defaultMessenger");
            }

            this.defaultMessenger = defaultMessenger;
        }

        public void Register<TMessage>(object recipient, Action<TMessage> action)
        {
            lock (SyncRoot)
            {
                this.defaultMessenger.Register(recipient, action);
            }

            System.Diagnostics.Debug.WriteLine("IMessenger.Register {0}", recipient);
        }

        public void Register<TMessage>(object recipient, object token, Action<TMessage> action)
        {
            lock (SyncRoot)
            {
                this.defaultMessenger.Register(recipient, token, action);
            }

            System.Diagnostics.Debug.WriteLine("IMessenger.Register {0} with token {1}", recipient, token);
        }

        public void Register<TMessage>(object recipient, object token, bool receiveDerivedMessagesToo, Action<TMessage> action)
        {
            lock (SyncRoot)
            {
                this.defaultMessenger.Register(recipient, token, receiveDerivedMessagesToo, action);
            }

            System.Diagnostics.Debug.WriteLine("IMessenger.Register {0} with token {1} include derived messages.", recipient, token);
        }

        public void Register<TMessage>(object recipient, bool receiveDerivedMessagesToo, Action<TMessage> action)
        {
            lock (SyncRoot)
            {
                this.defaultMessenger.Register(recipient, receiveDerivedMessagesToo, action);
            }

            System.Diagnostics.Debug.WriteLine("IMessenger.Register {0} include derived messages.", recipient);
        }

        public void Send<TMessage>(TMessage message)
        {
            this.defaultMessenger.Send(message);
            System.Diagnostics.Debug.WriteLine("IMessenger.Send {0}", message);
        }

        public void Send<TMessage, TTarget>(TMessage message)
        {
            this.defaultMessenger.Send<TMessage, TTarget>(message);
            System.Diagnostics.Debug.WriteLine("IMessenger.Send {0} to target {1}", message, typeof(TTarget).FullName);
        }

        public void Send<TMessage>(TMessage message, object token)
        {
            this.defaultMessenger.Send(message, token);
            System.Diagnostics.Debug.WriteLine("IMessenger.Send {0} with token {1}", message, token);
        }

        public void Unregister(object recipient)
        {
            lock (SyncRoot)
            {
                this.defaultMessenger.Unregister(recipient);
            }

            System.Diagnostics.Debug.WriteLine("IMessenger.Unregister {0}", recipient);
        }

        public void Unregister<TMessage>(object recipient)
        {
            lock (SyncRoot)
            {
                this.defaultMessenger.Unregister(recipient);
            }

            System.Diagnostics.Debug.WriteLine("IMessenger.Unregister {0}", recipient);
        }

        public void Unregister<TMessage>(object recipient, object token)
        {
            lock (SyncRoot)
            {
                this.defaultMessenger.Unregister<TMessage>(recipient, token);
            }

            System.Diagnostics.Debug.WriteLine("IMessenger.Unregister {0} with token {1}", recipient, token);
        }

        public void Unregister<TMessage>(object recipient, Action<TMessage> action)
        {
            lock (SyncRoot)
            {
                this.defaultMessenger.Unregister(recipient, action);
            }

            System.Diagnostics.Debug.WriteLine("IMessenger.Unregister {0}", recipient);
        }

        public void Unregister<TMessage>(object recipient, object token, Action<TMessage> action)
        {
            lock (SyncRoot)
            {
                this.defaultMessenger.Unregister(recipient, token, action);
            }

            System.Diagnostics.Debug.WriteLine("IMessenger.Unregister {0} with token {1}", recipient, token);
        }
    }
}