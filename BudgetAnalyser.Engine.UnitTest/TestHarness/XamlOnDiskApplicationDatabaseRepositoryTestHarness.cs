using System;
using BudgetAnalyser.Engine;
using JetBrains.Annotations;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.UnitTest.Helper;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.UnitTest.TestHarness
{
    public class XamlOnDiskApplicationDatabaseRepositoryTestHarness : XamlOnDiskApplicationDatabaseRepository
    {
        public XamlOnDiskApplicationDatabaseRepositoryTestHarness(
            [NotNull] BasicMapper<BudgetAnalyserStorageRoot, ApplicationDatabase> loadingMapper,
            [NotNull] BasicMapper<ApplicationDatabase, BudgetAnalyserStorageRoot> savingMapper) : base(loadingMapper, savingMapper)
        {
        }

        public Func<string, bool> FileExistsOverride { get; set; }
        public BudgetAnalyserStorageRoot StorageRootDto { get; private set; }
        public string StorageRootDtoSerialised { get; private set; }

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