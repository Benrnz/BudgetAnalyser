using System;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Services
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class ApplicationDatabaseService : IApplicationDatabaseService
    {
        private readonly IApplicationDatabaseRepository applicationRepository;
        private ApplicationDatabase budgetAnalyserDatabase;

        public ApplicationDatabaseService([NotNull] IApplicationDatabaseRepository applicationRepository)
        {
            if (applicationRepository == null)
            {
                throw new ArgumentNullException("applicationRepository");
            }

            this.applicationRepository = applicationRepository;
        }

        public ApplicationDatabase LoadPersistedStateData(MainApplicationStateModelV1 storedState)
        {
            if (storedState == null)
            {
                throw new ArgumentNullException("storedState");
            }

            // TODO Reconsider this when creating a new ApplicationDatabase is available from the repository.
            if (string.IsNullOrWhiteSpace(storedState.BudgetAnalyserDataStorageKey)) return null;

            this.budgetAnalyserDatabase = this.applicationRepository.Load(storedState);
            return this.budgetAnalyserDatabase;
        }

        public MainApplicationStateModelV1 PreparePersistentStateData()
        {
            if (this.budgetAnalyserDatabase == null)
            {
                return new MainApplicationStateModelV1();
            }

            return new MainApplicationStateModelV1
            {
                BudgetAnalyserDataStorageKey = this.budgetAnalyserDatabase.FileName
            };
        }
    }
}