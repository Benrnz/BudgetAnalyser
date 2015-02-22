using System;
using System.IO;
using System.Threading.Tasks;
using System.Xaml;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Persistence
{
    [AutoRegisterWithIoC]
    public class XamlOnDiskApplicationDatabaseRepository : IApplicationDatabaseRepository
    {
        private readonly BasicMapper<BudgetAnalyserStorageRoot, ApplicationDatabase> loadingMapper;

        public XamlOnDiskApplicationDatabaseRepository([NotNull] BasicMapper<BudgetAnalyserStorageRoot, ApplicationDatabase> loadingMapper)
        {
            if (loadingMapper == null)
            {
                throw new ArgumentNullException("loadingMapper");
            }

            this.loadingMapper = loadingMapper;
        }

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
            return db;
        }

        protected virtual bool FileExists(string budgetAnalyserDataStorage)
        {
            return File.Exists(budgetAnalyserDataStorage);
        }

        protected virtual string LoadXamlAsString(string fileName)
        {
            return File.ReadAllText(fileName);
        }

        protected async virtual Task<BudgetAnalyserStorageRoot> LoadXmlFromDiskAsync(string fileName)
        {
            object result = null;
            await Task.Run(() => result = XamlServices.Parse(LoadXamlAsString(fileName)));
            return result as BudgetAnalyserStorageRoot;
        }
    }
}