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

        public ApplicationDatabase LoadPersistedStateData(MainApplicationStateModel storedState)
        {
            this.budgetAnalyserDatabase = this.applicationRepository.Load(storedState);
            return this.budgetAnalyserDatabase;
        }

        public MainApplicationStateModel PreparePersistentStateData()
        {
            return new MainApplicationStateModel
            {
                BudgetAnalyserDataStorage = this.budgetAnalyserDatabase.FileName,
            };
        }
    }
}