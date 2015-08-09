using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xaml;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;

namespace BudgetAnalyser.Engine.Persistence
{
    [AutoRegisterWithIoC]
    public class XamlOnDiskApplicationDatabaseRepository : IApplicationDatabaseRepository
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

        public async Task<ApplicationDatabase> CreateNewAsync(string storageKey)
        {
            if (storageKey.IsNothing())
            {
                throw new ArgumentNullException("storageKey");
            }

            string path = Path.Combine(Path.GetDirectoryName(storageKey), Path.GetFileNameWithoutExtension(storageKey));
            var storageRoot = new BudgetAnalyserStorageRoot
            {
                BudgetCollectionRootDto = new StorageBranch { Source = path + ".Budget.xml" },
                LedgerBookRootDto = new StorageBranch { Source = path + ".LedgerBook.xml" },
                LedgerReconciliationToDoCollection = new List<ToDoTaskDto>(),
                MatchingRulesCollectionRootDto = new StorageBranch { Source = path + ".MatchingRules.xml" },
                StatementModelRootDto = new StorageBranch { Source = path + ".Transactions.csv" }
            };
            string serialised = Serialise(storageRoot);
            await WriteToDiskAsync(storageKey, serialised);
            ApplicationDatabase appDb = this.loadingMapper.Map(storageRoot);
            appDb.FileName = storageKey;
            return appDb;
        }

        public async Task<ApplicationDatabase> LoadAsync(string storageKey)
        {
            if (storageKey.IsNothing())
            {
                throw new ArgumentNullException("storageKey");
            }

            string fileName = storageKey;
            if (!FileExists(fileName))
            {
                throw new KeyNotFoundException("File does not exist.");
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

        public async Task SaveAsync(ApplicationDatabase budgetAnalyserDatabase)
        {
            if (budgetAnalyserDatabase == null)
            {
                throw new ArgumentNullException("budgetAnalyserDatabase");
            }

            string serialised = Serialise(this.savingMapper.Map(budgetAnalyserDatabase));
            await WriteToDiskAsync(budgetAnalyserDatabase.FileName, serialised);
        }

        protected virtual bool FileExists(string budgetAnalyserDataStorage)
        {
            return File.Exists(budgetAnalyserDataStorage);
        }

        protected virtual string LoadXamlAsString(string fileName)
        {
            return File.ReadAllText(fileName);
        }

        protected virtual string Serialise(BudgetAnalyserStorageRoot budgetAnalyserDatabase)
        {
            return XamlServices.Save(budgetAnalyserDatabase);
        }

        protected virtual async Task WriteToDiskAsync(string fileName, string data)
        {
            using (var file = new StreamWriter(fileName, false))
            {
                await file.WriteAsync(data);
                await file.FlushAsync();
            }
        }

        private async Task<BudgetAnalyserStorageRoot> LoadXmlFromDiskAsync(string fileName)
        {
            object result = null;
            await Task.Run(() => result = XamlServices.Parse(LoadXamlAsString(fileName)));
            return result as BudgetAnalyserStorageRoot;
        }
    }
}