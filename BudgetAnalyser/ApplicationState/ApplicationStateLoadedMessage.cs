using System.Collections.ObjectModel;
using BudgetAnalyser.Engine.Persistence;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.ApplicationState
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
        public ApplicationStateLoadedMessage(IEnumerable<IPersistentApplicationStateObject> rehydratedModels)
        {
            var removeDuplicates = rehydratedModels
                .GroupBy(model => model.GetType(), model => model)
                .Where(group => group.Key is not null)
                .Select(group => group.First());

            RehydratedModels =
                new ReadOnlyDictionary<Type, IPersistentApplicationStateObject>(removeDuplicates.ToDictionary(m => m.GetType(), m => m));
        }

        /// <summary>
        ///     Gets the recently loaded user data from persistent storage that this message should carry to all subscribers.
        /// </summary>
        public IReadOnlyDictionary<Type, IPersistentApplicationStateObject> RehydratedModels { get; private set; }

        /// <summary>
        ///     Retrieves an element of the type <typeparamref name="T" />.
        ///     If no element of the requested type exists in the <see cref="RehydratedModels" /> dictionary, null is returned.
        /// </summary>
        /// <typeparam name="T">The concrete type that implements <see cref="IPersistentApplicationStateObject" /> to retrieve.</typeparam>
        public T? ElementOfType<T>() where T : class, IPersistentApplicationStateObject
        {
            var type = typeof(T);
            return RehydratedModels.TryGetValue(type, out var model) ? model as T : null;
        }
    }
}
