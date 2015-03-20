using System;
using System.IO;
using System.Threading.Tasks;
using System.Xaml;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Ledger;

namespace BudgetAnalyser.Engine.Persistence
{
    [AutoRegisterWithIoC]
    public class XamlOnDiskApplicationDatabaseRepository : IApplicationDatabaseRepository, IApplicationHookEventPublisher
    {
        private readonly BasicMapper<BudgetAnalyserStorageRoot, ApplicationDatabase> loadingMapper;
        private readonly BasicMapper<ApplicationDatabase, BudgetAnalyserStorageRoot> savingMapper;

        public XamlOnDiskApplicationDatabaseRepository(
            [NotNull] BasicMapper<BudgetAnalyserStorageRoot, ApplicationDatabase> loadingMapper,
            [NotNull] BasicMapper<ApplicationDatabase, BudgetAnalyserStorageRoot> savingMapper)
        {
            if (loadingMapper == null)
            {
                throw new ArgumentNullException("loadingMapper");
            }

            if (savingMapper == null)
            {
                throw new ArgumentNullException("savingMapper");
            }

            this.loadingMapper = loadingMapper;
            this.savingMapper = savingMapper;
        }

        public event EventHandler<ApplicationHookEventArgs> ApplicationEvent;

        public async Task<ApplicationDatabase> LoadAsync(string storageKey)
        {
            if (string.IsNullOrWhiteSpace(storageKey))
            {
                throw new ArgumentNullException("storageKey");
            }

            string fileName = storageKey;
            if (!FileExists(fileName))
            {
                throw new NotImplementedException("TODO Creating new Application Database still to come.");
            }

            BudgetAnalyserStorageRoot storageRoot;
            try
            {
                storageRoot = await LoadXmlFromDiskAsync(fileName);
            }
            catch (Exception ex)
            {
                throw new DataFormatException("Deserialisation Application Database file failed, an exception was thrown by the Xml deserialiser, the file format is invalid.", ex);
            }

            ApplicationDatabase db = this.loadingMapper.Map(storageRoot);
            db.FileName = fileName;
            if (db.LedgerReconciliationToDoCollection == null)
            {
                db.LedgerReconciliationToDoCollection = new ToDoCollection();
            }
            return db;
        }

        public void Save(ApplicationDatabase budgetAnalyserDatabase)
        {
            if (budgetAnalyserDatabase == null)
            {
                throw new ArgumentNullException("budgetAnalyserDatabase");
            }

            string serialised = Serialise(this.savingMapper.Map(budgetAnalyserDatabase));
            WriteToDisk(budgetAnalyserDatabase.FileName, serialised);

            EventHandler<ApplicationHookEventArgs> handler = ApplicationEvent;
            if (handler != null)
            {
                handler(this, new ApplicationHookEventArgs(ApplicationHookEventType.Repository, "ApplicationDatabaseRepository", ApplicationHookEventArgs.Save));
            }
        }

        protected virtual bool FileExists(string budgetAnalyserDataStorage)
        {
            return File.Exists(budgetAnalyserDataStorage);
        }

        protected virtual string LoadXamlAsString(string fileName)
        {
            return File.ReadAllText(fileName);
        }

        protected virtual async Task<BudgetAnalyserStorageRoot> LoadXmlFromDiskAsync(string fileName)
        {
            object result = null;
            await Task.Run(() => result = XamlServices.Parse(LoadXamlAsString(fileName)));
            return result as BudgetAnalyserStorageRoot;
        }

        protected virtual string Serialise(BudgetAnalyserStorageRoot budgetAnalyserDatabase)
        {
            return XamlServices.Save(budgetAnalyserDatabase);
        }

        protected virtual void WriteToDisk(string fileName, string data)
        {
            File.WriteAllText(fileName, data);
        }
    }
}