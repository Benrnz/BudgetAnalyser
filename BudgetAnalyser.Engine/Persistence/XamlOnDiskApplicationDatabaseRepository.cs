using System;
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

        public ApplicationDatabase Load(MainApplicationStateModel stateModel)
        {
            var fileName = stateModel.BudgetAnalyserDataStorage;
            if (!FileExists(fileName))
            {
                throw new NotImplementedException("TODO Creating new Application Database still to come.");
            }

            BudgetAnalyserStorageRoot storageRoot;
            try
            {
                storageRoot = LoadXmlFromDisk(fileName);
            }
            catch (Exception ex)
            {
                throw new DataFormatException("Deserialisation Application Database file failed, an exception was thrown by the Xml deserialiser, the file format is invalid.", ex);
            }

            var db = this.loadingMapper.Map(storageRoot);
            db.FileName = fileName;
            return db;
        }

        protected virtual string LoadXamlAsString(string fileName)
        {
            return System.IO.File.ReadAllText(fileName);
        }

        protected virtual bool FileExists(string budgetAnalyserDataStorage)
        {
            return System.IO.File.Exists(budgetAnalyserDataStorage);
        }

        protected virtual BudgetAnalyserStorageRoot LoadXmlFromDisk(string fileName)
        {
            return XamlServices.Parse(LoadXamlAsString(fileName)) as BudgetAnalyserStorageRoot;
        }
    }
}