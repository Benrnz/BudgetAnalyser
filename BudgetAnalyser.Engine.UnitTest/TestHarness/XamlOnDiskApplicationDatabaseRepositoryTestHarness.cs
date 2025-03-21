using BudgetAnalyser.Engine.Persistence;
using JetBrains.Annotations;
using Portable.Xaml;
using Rees.UnitTestUtilities;

namespace BudgetAnalyser.Engine.UnitTest.TestHarness;

public class XamlOnDiskApplicationDatabaseRepositoryTestHarness : XamlOnDiskApplicationDatabaseRepository
{
    public XamlOnDiskApplicationDatabaseRepositoryTestHarness([NotNull] IDtoMapper<BudgetAnalyserStorageRoot, ApplicationDatabase> mapper) : base(mapper)
    {
    }

    public Func<string, bool> FileExistsOverride { get; set; }
    public BudgetAnalyserStorageRoot StorageRootDto { get; private set; }
    public string StorageRootDtoSerialised { get; private set; }

    protected override bool FileExists(string budgetAnalyserDataStorage)
    {
        return FileExistsOverride is null ? base.FileExists(budgetAnalyserDataStorage) : FileExistsOverride(budgetAnalyserDataStorage);
    }

    protected override string LoadXamlAsString(string fileName)
    {
        StorageRootDtoSerialised = GetType().Assembly.ExtractEmbeddedResourceAsText(fileName, true);
        StorageRootDto = XamlServices.Parse(StorageRootDtoSerialised) as BudgetAnalyserStorageRoot;
        return StorageRootDtoSerialised;
    }
}
