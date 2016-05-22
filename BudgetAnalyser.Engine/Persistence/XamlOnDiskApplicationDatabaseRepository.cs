using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using JetBrains.Annotations;
using Portable.Xaml;
using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Engine.Persistence
{
    /// <summary>
    ///     A repository to store the top level Budget Analyser object graph in Xaml format on local disk.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Persistence.IApplicationDatabaseRepository" />
    [AutoRegisterWithIoC]
    public class XamlOnDiskApplicationDatabaseRepository : IApplicationDatabaseRepository
    {
        private readonly IDtoMapper<BudgetAnalyserStorageRoot, ApplicationDatabase> mapper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="XamlOnDiskApplicationDatabaseRepository" /> class.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public XamlOnDiskApplicationDatabaseRepository(
            [NotNull] IDtoMapper<BudgetAnalyserStorageRoot, ApplicationDatabase> mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            this.mapper = mapper;
        }

        /// <summary>
        ///     Creates a new budget analyser database graph.
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        public async Task<ApplicationDatabase> CreateNewAsync(string storageKey)
        {
            if (storageKey.IsNothing())
            {
                throw new ArgumentNullException(nameof(storageKey));
            }

            var path = Path.Combine(Path.GetDirectoryName(storageKey) ?? string.Empty,
                Path.GetFileNameWithoutExtension(storageKey) ?? string.Empty);
            var storageRoot = new BudgetAnalyserStorageRoot
            {
                BudgetCollectionRootDto = new StorageBranch { Source = path + ".Budget.xml" },
                LedgerBookRootDto = new StorageBranch { Source = path + ".LedgerBook.xml" },
                LedgerReconciliationToDoCollection = new List<ToDoTaskDto>(),
                MatchingRulesCollectionRootDto = new StorageBranch { Source = path + ".MatchingRules.xml" },
                StatementModelRootDto = new StorageBranch { Source = path + ".Transactions.csv" }
            };
            var serialised = Serialise(storageRoot);
            await WriteToDiskAsync(storageKey, serialised);
            var appDb = this.mapper.ToModel(storageRoot);
            appDb.FileName = storageKey;
            return appDb;
        }

        /// <summary>
        ///     Loads the Budget Analyser database graph from persistent storage.
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">File does not exist.</exception>
        /// <exception cref="DataFormatException">
        ///     Deserialisation Application Database file failed, an exception was thrown by the
        ///     Xml deserialiser, the file format is invalid.
        /// </exception>
        public async Task<ApplicationDatabase> LoadAsync(string storageKey)
        {
            if (storageKey.IsNothing())
            {
                throw new ArgumentNullException(nameof(storageKey));
            }

            var fileName = storageKey;
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
                throw new DataFormatException(
                    "Deserialisation Application Database file failed, an exception was thrown by the Xml deserialiser, the file format is invalid.",
                    ex);
            }

            var db = this.mapper.ToModel(storageRoot);
            db.FileName = fileName;
            if (db.LedgerReconciliationToDoCollection == null)
            {
                db.LedgerReconciliationToDoCollection = new ToDoCollection();
            }
            return db;
        }

        /// <summary>
        ///     Saves the Budget Analyser database graph to persistent storage.
        /// </summary>
        /// <param name="budgetAnalyserDatabase">The budget analyser database.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public async Task SaveAsync(ApplicationDatabase budgetAnalyserDatabase)
        {
            if (budgetAnalyserDatabase == null)
            {
                throw new ArgumentNullException(nameof(budgetAnalyserDatabase));
            }

            var serialised = Serialise(this.mapper.ToDto(budgetAnalyserDatabase));
            await WriteToDiskAsync(budgetAnalyserDatabase.FileName, serialised);
        }

        /// <summary>
        ///     Checks to see if the budget analyser identified by the <paramref name="budgetAnalyserDataStorage" /> exists in
        ///     storage or not.
        /// </summary>
        /// <param name="budgetAnalyserDataStorage">The budget analyser data storage.</param>
        protected virtual bool FileExists(string budgetAnalyserDataStorage)
        {
            return File.Exists(budgetAnalyserDataStorage);
        }

        /// <summary>
        ///     Loads the xaml as a string.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        protected virtual string LoadXamlAsString(string fileName)
        {
            return File.ReadAllText(fileName);
        }

        /// <summary>
        ///     Serialises the specified budget analyser database to a Xaml string.
        /// </summary>
        /// <param name="budgetAnalyserDatabase">The budget analyser database.</param>
        protected virtual string Serialise(BudgetAnalyserStorageRoot budgetAnalyserDatabase)
        {
            return XamlServices.Save(budgetAnalyserDatabase);
        }

        /// <summary>
        ///     Writes the data to local disk.
        /// </summary>
        protected virtual async Task WriteToDiskAsync(string fileName, string data)
        {
            using (var stream = new FileStream(fileName, FileMode.Create))
            {
                using (var file = new StreamWriter(stream))
                {
                    await file.WriteAsync(data);
                    await file.FlushAsync();
                }
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