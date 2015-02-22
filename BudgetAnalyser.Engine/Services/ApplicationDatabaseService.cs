using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            this.databaseDependendants = databaseDependendants.OrderBy(d => d.Sequence).ToList();
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
        public ApplicationDatabase Close()
        {
            foreach (IApplicationDatabaseDependant service in this.databaseDependendants.OrderByDescending(d => d.Sequence))
            {
                service.Close();
            }

            ClearDirtyDataFlags();

            this.budgetAnalyserDatabase.Close();
            return this.budgetAnalyserDatabase;
        }

        /// <summary>
        ///     Loads the specified Budget Analyser file by file name.
        ///     No warning will be given if there is any unsaved data. This should be checked before calling this method.
        /// </summary>
        /// <param name="storageKey">Name and path to the file.</param>
        public async Task<ApplicationDatabase> Load(string storageKey)
        {
            if (string.IsNullOrWhiteSpace(storageKey))
            {
                throw new ArgumentNullException("storageKey");
            }

            ClearDirtyDataFlags();

            this.budgetAnalyserDatabase = await this.applicationRepository.LoadAsync(storageKey);
            try
            {
                foreach (IApplicationDatabaseDependant service in this.databaseDependendants) // Already sorted ascending by sequence number.
                {
                    await service.LoadAsync(this.budgetAnalyserDatabase);
                }
            }
            catch (DataFormatException ex)
            {
                Close();
                throw new DataFormatException("A subordindate data file is invalid or corrupt unable to load " + storageKey, ex);
            }
            catch (KeyNotFoundException ex)
            {
                Close();
                throw new KeyNotFoundException("A subordinate data file cannot be found: " + ex.Message, ex);
            }
            catch (NotSupportedException ex)
            {
                Close();
                throw new DataFormatException("A subordinate data file contains unsupported data.", ex);
            }

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
            // TODO Validate before save
            // TODO Save data only when valid

            ClearDirtyDataFlags();
        }

        private void ClearDirtyDataFlags()
        {
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