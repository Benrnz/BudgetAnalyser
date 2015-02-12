using System;
using System.IO;
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

        public ApplicationDatabase Load(MainApplicationStateModelV1 stateModel)
        {
            string fileName = stateModel.BudgetAnalyserDataStorageKey;
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

        protected virtual BudgetAnalyserStorageRoot LoadXmlFromDisk(string fileName)
        {
            return XamlServices.Parse(LoadXamlAsString(fileName)) as BudgetAnalyserStorageRoot;
        }
    }
}