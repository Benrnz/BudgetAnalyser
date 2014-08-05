using System.Collections.Generic;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    /// An interface to desribe an event sink (listener/subscriber) for application-wdie events.
    /// </summary>
    public interface IApplicationHookSubscriber
    {
        /// <summary>
        /// An initialisation method to wire up the listener to any number of event-publishers.
        /// This will be called during application start-up to allow the listener (this instance) to
        /// begin listening for application events.
        /// </summary>
        /// <param name="publishers">
        /// All known event publishers. This is intended to be populated automatically
        /// by the IoC container.
        /// </param>
        void Subscribe(IEnumerable<IApplicationHookEventPublisher> publishers);
    }
}