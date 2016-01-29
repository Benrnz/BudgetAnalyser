using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Persistence;
using GalaSoft.MvvmLight.Messaging;

namespace BudgetAnalyser.Uwp.ApplicationState
{
    /// <summary>
    /// A <see cref="MessageBase"/> message object that broadcasts to subscribed components that the application is about to save user state data.  In response to this message 
    /// a subscribed component should add any data it wishes to persist.  Persistent data is not additive, if no data is added any existing persisted data is lost.
    /// </summary>
    public class ApplicationStateRequestedMessage : MessageBase
    {
        private readonly List<IPersistentApplicationState> modelsToPersist = new List<IPersistentApplicationState>();

        /// <summary>
        /// Gets all the persistent data for all subscribers.
        /// </summary>
        public IReadOnlyCollection<IPersistentApplicationState> PersistentData => this.modelsToPersist;

        /// <summary>
        /// Each subscriber can call this method to include any data it wishes to persist.  This data is intended to be user state metadata. It is not intended to be used for
        /// wholesale application database data.
        /// </summary>
        /// <param name="model">
        /// The model to persist.  The type of this model will be used as a key and when data is loaded back from persistent storage this key (type) will be used
        /// to retrieve it.  No other component should use this type, or collisions will occur.
        /// </param>
        /// <exception cref="DuplicateNameException">Attempt to save application state with a model that has already been saved.</exception>
        public void PersistThisModel(IPersistentApplicationState model)
        {
            if (this.modelsToPersist.Any(m => m.GetType() == model.GetType()))
            {
                throw new DuplicateNameException("Attempt to save application state with a model that has already been saved.");
            }

            this.modelsToPersist.Add(model);
        }
    }
}