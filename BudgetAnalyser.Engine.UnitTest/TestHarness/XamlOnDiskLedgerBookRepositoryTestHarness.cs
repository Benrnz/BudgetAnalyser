using System.Diagnostics;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.UnitTest.Helper;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.UnitTest.TestHarness;

internal class XamlOnDiskLedgerBookRepositoryTestHarness : XamlOnDiskLedgerBookRepository
{
    public XamlOnDiskLedgerBookRepositoryTestHarness(
        [NotNull]
        IDtoMapper<LedgerBookDto, LedgerBook> mapper,
        IReaderWriterSelector selector) : base(mapper, new BankImportUtilitiesTestHarness(), selector)
    {
        LoadXamlFromDiskFromEmbeddedResources = true;
    }

    public event EventHandler DtoDeserialised;
    public Func<string, bool> FileExistsOverride { get; set; }
    public LedgerBookDto LedgerBookDto { get; private set; }
    public Func<string, string> LoadXamlAsStringOverride { get; set; }
    public bool LoadXamlFromDiskFromEmbeddedResources { get; set; }
    public Action<LedgerBookDto> SaveDtoToDiskOverride { get; set; }

    public string SerialisedData { get; private set; }

    protected override string LoadXamlAsString(string fileName)
    {
        if (LoadXamlAsStringOverride is null)
        {
            var result = base.LoadXamlAsString(fileName);
            Debug.WriteLine(result);
            return result;
        }

        return LoadXamlAsStringOverride(fileName);
    }

    protected override async Task<LedgerBookDto> LoadXamlFromDiskAsync(string fileName, bool isEncrypted)
    {
        if (LoadXamlFromDiskFromEmbeddedResources)
        {
            LedgerBookDto = GetType().Assembly.ExtractEmbeddedResourceAsXamlObject<LedgerBookDto>(fileName, true);
            var handler = DtoDeserialised;
            handler?.Invoke(this, EventArgs.Empty);

            return LedgerBookDto;
        }

        LedgerBookDto = await base.LoadXamlFromDiskAsync(fileName, isEncrypted);
        return LedgerBookDto;
    }

    protected override async Task SaveDtoToDiskAsync(LedgerBookDto dataEntity, bool isEncrypted)
    {
        if (SaveDtoToDiskOverride is null)
        {
            await base.SaveDtoToDiskAsync(dataEntity, isEncrypted);
            return;
        }

        SaveDtoToDiskOverride(dataEntity);
    }

    protected override string Serialise(LedgerBookDto dataEntity)
    {
        SerialisedData = base.Serialise(dataEntity);
        return SerialisedData;
    }
}
