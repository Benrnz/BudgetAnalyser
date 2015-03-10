using System;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Persistence;
using GalaSoft.MvvmLight.Messaging;

namespace BudgetAnalyser
{
    public class ApplicationDatabaseReadyMessage : MessageBase
    {
        public ApplicationDatabaseReadyMessage([NotNull] ApplicationDatabase appDb)
        {
            if (appDb == null)
            {
                throw new ArgumentNullException("appDb");
            }

            ApplicationDatabase = appDb;
        }

        public ApplicationDatabase ApplicationDatabase { get; private set; }
    }
}
