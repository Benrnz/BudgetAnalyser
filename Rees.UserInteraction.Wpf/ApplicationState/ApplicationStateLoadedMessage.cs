using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight.Messaging;
using Rees.Wpf.Contracts;

namespace Rees.Wpf.ApplicationState
{
    /// <summary>
    ///     A <see cref="MessageBase" /> message object that contains recently loaded user data to be broadcast to subscribing
    ///     components.
    ///     This message will be broadcast with the <see cref="IMessenger" /> after the <see cref="IPersistApplicationState" />
    ///     implementation has read the data from persistent storage.
    /// </summary>
    public class ApplicationStateLoadedMessage : MessageBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ApplicationStateLoadedMessage" /> class.
        /// </summary>
        /// <param name="rehydratedModels">
        ///     The recently loaded user data from persistent storage that this message should carry to
        ///     all subscribers.
        /// </param>
        public ApplicationStateLoadedMessage(IEnumerable<IPersistent> rehydratedModels)
        {
            var removeDuplicates = rehydratedModels
                .GroupBy(model => model.GetType(), model => model)
                .Where(group => group.Key != null)
                .Select(group => group.First());

            RehydratedModels =
                new ReadOnlyDictionary<Type, IPersistent>(removeDuplicates.ToDictionary(m => m.GetType(), m => m));
        }

        /// <summary>
        ///     Gets the recently loaded user data from persistent storage that this message should carry to all subscribers.
        /// </summary>
        public IReadOnlyDictionary<Type, IPersistent> RehydratedModels { get; private set; }

        /// <summary>
        ///     Retrieves an element of the type <typeparamref name="T" />.
        ///     If no element of the requested type exists in the <see cref="RehydratedModels" /> dictionary, null is returned.
        /// </summary>
        /// <typeparam name="T">The concrete type that implements <see cref="IPersistent" /> to retrieve.</typeparam>
        public T ElementOfType<T>() where T : class, IPersistent
        {
            var type = typeof (T);
            if (RehydratedModels.ContainsKey(type))
            {
                return RehydratedModels[type] as T;
            }

            return null;
        }
    }
}