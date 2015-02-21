using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Services
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class ApplicationDatabaseService : IApplicationDatabaseService
    {
        private readonly IApplicationDatabaseRepository applicationRepository;
        private readonly IEnumerable<IApplicationDatabaseDependant> databaseDependendants;
        private readonly Dictionary<ApplicationDataType, bool> dirtyData = new Dictionary<ApplicationDataType, bool>();
        private ApplicationDatabase budgetAnalyserDatabase;

        public ApplicationDatabaseService(
            [NotNull] IApplicationDatabaseRepository applicationRepository,
            [NotNull] IEnumerable<IApplicationDatabaseDependant> databaseDependendants)

        {
            if (applicationRepository == null)
            {
                throw new ArgumentNullException("applicationRepository");
            }

            if (databaseDependendants == null)
            {
                throw new ArgumentNullException("databaseDependendants");
            }

            this.applicationRepository = applicationRepository;
            this.databaseDependendants = databaseDependendants;
            InitialiseDirtyDataTable();
        }

        /// <summary>
        ///     Gets or sets a value indicating whether there are unsaved changes across all application data.
        /// </summary>
        public bool HasUnsavedChanges
        {
            get { return this.dirtyData.Values.Any(v => v); }
        }

        /// <summary>
        ///     Closes the currently loaded Budget Analyser file, and therefore any other application data is also closed.
        ///     Changes are discarded, no prompt or error will occur if there are unsaved changes. This check should be done before
        ///     calling this method.
        /// </summary>
        public void Close()
        {
            foreach (IApplicationDatabaseDependant service in this.databaseDependendants)
            {
                service.Close();
            }

            foreach (int value in Enum.GetValues(typeof(ApplicationDataType)))
            {
                var enumValue = (ApplicationDataType)value;
                this.dirtyData[enumValue] = false;
            }
        }

        public ApplicationDatabase LoadPersistedStateData(MainApplicationStateModelV1 storedState)
        {
            if (storedState == null)
            {
                throw new ArgumentNullException("storedState");
            }

            // TODO Reconsider this when creating a new ApplicationDatabase is available from the repository.
            if (string.IsNullOrWhiteSpace(storedState.BudgetAnalyserDataStorageKey))
            {
                return null;
            }

            this.budgetAnalyserDatabase = this.applicationRepository.Load(storedState);
            return this.budgetAnalyserDatabase;
        }

        /// <summary>
        ///     Notifies the service that data has changed and will need to be saved.
        /// </summary>
        public void NotifyOfChange(ApplicationDataType dataType)
        {
            this.dirtyData[dataType] = true;
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

        /// <summary>
        ///     Saves all Budget Analyser application data.
        /// </summary>
        public void Save()
        {
            // TODO
            foreach (ApplicationDataType key in this.dirtyData.Keys)
            {
                this.dirtyData[key] = false;
            }
        }

        private void InitialiseDirtyDataTable()
        {
            foreach (int value in Enum.GetValues(typeof(ApplicationDataType)))
            {
                var enumValue = (ApplicationDataType)value;
                this.dirtyData.Add(enumValue, false);
            }
        }
    }
}