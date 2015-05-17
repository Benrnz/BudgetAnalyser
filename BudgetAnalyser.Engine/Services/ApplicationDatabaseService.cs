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
        private readonly IEnumerable<ISupportsModelPersistence> databaseDependents;
        private readonly Dictionary<ApplicationDataType, bool> dirtyData = new Dictionary<ApplicationDataType, bool>();
        private ApplicationDatabase budgetAnalyserDatabase;

        public ApplicationDatabaseService(
            [NotNull] IApplicationDatabaseRepository applicationRepository,
            [NotNull] IEnumerable<ISupportsModelPersistence> databaseDependents)
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

        public bool HasUnsavedChanges
        {
            get { return this.dirtyData.Values.Any(v => v); }
        }

        public ApplicationDatabase Close()
        {
            if (this.budgetAnalyserDatabase == null)
            {
                return null;
            }

            foreach (ISupportsModelPersistence service in this.databaseDependents.OrderByDescending(d => d.LoadSequence))
            {
                service.Close();
            }

            ClearDirtyDataFlags();

            this.budgetAnalyserDatabase.Close();
            return this.budgetAnalyserDatabase;
        }

        public async Task<ApplicationDatabase> CreateNewDatabaseAsync(string storageKey)
        {
            if (storageKey.IsNothing())
            {
                throw new ArgumentNullException("storageKey");
            }

            ClearDirtyDataFlags();
            this.budgetAnalyserDatabase = await this.applicationRepository.CreateNewAsync(storageKey);
            foreach (ISupportsModelPersistence service in this.databaseDependents)
            {
                await service.CreateAsync(this.budgetAnalyserDatabase);
            }

            return this.budgetAnalyserDatabase;
        }

        public async Task<ApplicationDatabase> LoadAsync(string storageKey)
        {
            if (storageKey.IsNothing())
            {
                throw new ArgumentNullException("storageKey");
            }

            ClearDirtyDataFlags();

            this.budgetAnalyserDatabase = await this.applicationRepository.LoadAsync(storageKey);
            try
            {
                foreach (ISupportsModelPersistence service in this.databaseDependents) // Already sorted ascending by sequence number.
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

            var contexts = new Dictionary<ApplicationDataType, object>();
            this.databaseDependents
                .Where(service => this.dirtyData[service.DataType])
                .ToList()
                .ForEach(service => service.SavePreview(contexts));

            // Save the main application repository first.
            await this.applicationRepository.SaveAsync(this.budgetAnalyserDatabase);

            // Save all remaining service's data in parallel.
            await this.databaseDependents
                .Where(service => this.dirtyData[service.DataType])
                .Select(async service => await Task.Run(() => service.SaveAsync(contexts)))
                .ContinueWhenAllTasksComplete();

            ClearDirtyDataFlags();
        }

        public bool ValidateAll([NotNull] StringBuilder messages)
        {
            if (messages == null)
            {
                throw new ArgumentNullException("messages");
            }

            var valid = true;
            foreach (ISupportsModelPersistence service in this.databaseDependents) // Already sorted ascending by sequence number.
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