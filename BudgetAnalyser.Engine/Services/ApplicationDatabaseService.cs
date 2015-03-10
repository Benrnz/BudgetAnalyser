using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Services
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class ApplicationDatabaseService : IApplicationDatabaseService
    {
        private readonly IApplicationDatabaseRepository applicationRepository;
        private readonly IEnumerable<IApplicationDatabaseDependent> databaseDependents;
        private readonly Dictionary<ApplicationDataType, bool> dirtyData = new Dictionary<ApplicationDataType, bool>();
        private ApplicationDatabase budgetAnalyserDatabase;

        public ApplicationDatabaseService(
            [NotNull] IApplicationDatabaseRepository applicationRepository,
            [NotNull] IEnumerable<IApplicationDatabaseDependent> databaseDependents)
        {
            if (applicationRepository == null)
            {
                throw new ArgumentNullException("applicationRepository");
            }

            if (databaseDependents == null)
            {
                throw new ArgumentNullException("databaseDependents");
            }

            this.applicationRepository = applicationRepository;
            this.databaseDependents = databaseDependents.OrderBy(d => d.LoadSequence).ToList();
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
            foreach (IApplicationDatabaseDependent service in this.databaseDependents.OrderByDescending(d => d.LoadSequence))
            {
                service.Close();
            }

            ClearDirtyDataFlags();

            this.budgetAnalyserDatabase.Close();
        }

        /// <summary>
        ///     Loads the specified Budget Analyser file by file name.
        ///     No warning will be given if there is any unsaved data. This should be checked before calling this method.
        /// </summary>
        /// <param name="storageKey">Name and path to the file.</param>
        public async Task<ApplicationDatabase> LoadAsync(string storageKey)
        {
            if (string.IsNullOrWhiteSpace(storageKey))
            {
                throw new ArgumentNullException("storageKey");
            }

            ClearDirtyDataFlags();

            this.budgetAnalyserDatabase = await this.applicationRepository.LoadAsync(storageKey);
            try
            {
                foreach (IApplicationDatabaseDependent service in this.databaseDependents) // Already sorted ascending by sequence number.
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
        public async Task SaveAsync()
        {
            if (this.budgetAnalyserDatabase == null)
            {
                throw new InvalidOperationException("Application Database cannot be null here. Code Bug.");
            }

            if (!HasUnsavedChanges)
            {
                return;
            }

            var messages = new StringBuilder();
            if (!ValidateAll(messages))
            {
                throw new ValidationWarningException(messages.ToString());
            }

            this.budgetAnalyserDatabase.LedgerReconciliationToDoCollection.Clear(); // Only clears system generated tasks, not persistent user created tasks.

            // Save the main application repository first.
            await this.applicationRepository.SaveAsync(this.budgetAnalyserDatabase);

            // Save all remaining service's data in parallel.
            var savingTasks = this.databaseDependents
                .Where(service => this.dirtyData[service.DataType])
                .Select(service => Task.Run(() => service.SaveAsync()))
                .ToList();

            await Task.WhenAll(savingTasks);
            ClearDirtyDataFlags();
        }

        public bool ValidateAll(StringBuilder messages)
        {
            var valid = true;
            foreach (IApplicationDatabaseDependent service in this.databaseDependents) // Already sorted ascending by sequence number.
            {
                try
                {
                    valid = service.ValidateModel(messages);
                }
                catch (ValidationWarningException ex)
                {
                    messages.AppendLine(ex.Message);
                    valid = false;
                }
            }

            return valid;
        }

        private void ClearDirtyDataFlags()
        {
            foreach (ApplicationDataType key in this.dirtyData.Keys.ToList())
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