using System;
using GalaSoft.MvvmLight.Messaging;

namespace BudgetAnalyser
{
    internal static class MessagingGate
    {
        private static readonly object SyncRoot = new object();

        public static void Register<T>(object recipient, Action<T> handler) where T : MessageBase
        {
            lock (SyncRoot)
            {
                Messenger.Default.Register(recipient, handler);
            }
        }

        /// <summary>
        ///     Sends the specified message.
        ///     Use this when you are concerned about concurrency. Sending messages at the same time as registering new listeners
        ///     on many threads will cause issues.
        /// </summary>
        /// <typeparam name="T">The message to send</typeparam>
        /// <param name="message">The message.</param>
        public static void Send<T>(T message)
        {
            lock (SyncRoot)
            {
                Messenger.Default.Send(message);
            }
        }

        public static void Unregister<T>(object recipient) where T : MessageBase
        {
            lock (SyncRoot)
            {
                Messenger.Default.Unregister<T>(recipient);
            }
        }
    }
}