using System;
using System.Xaml;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.UnitTest.Helper;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class XamlOnDiskApplicationDatabaseRepositoryTestHarness : XamlOnDiskApplicationDatabaseRepository
    {
        public XamlOnDiskApplicationDatabaseRepositoryTestHarness(
            [NotNull] BasicMapper<BudgetAnalyserStorageRoot, ApplicationDatabase> loadingMapper, 
            [NotNull] BasicMapper<ApplicationDatabase, BudgetAnalyserStorageRoot> savingMapper) : base(loadingMapper, savingMapper)
        {
        }

        public BudgetAnalyserStorageRoot StorageRootDto { get; private set; }
        public string StorageRootDtoSerialised { get; private set; }
        public Func<string, bool> FileExistsOverride { get; set; }

        protected override bool FileExists(string budgetAnalyserDataStorage)
        {
            if (FileExistsOverride == null)
            {
                return base.FileExists(budgetAnalyserDataStorage);
            }

            return FileExistsOverride(budgetAnalyserDataStorage);
        }

        protected override string LoadXamlAsString(string fileName)
        {
            StorageRootDtoSerialised = EmbeddedResourceHelper.ExtractText(fileName, true);
            StorageRootDto = XamlServices.Parse(StorageRootDtoSerialised) as BudgetAnalyserStorageRoot;
            return StorageRootDtoSerialised;
        }
    }
}
