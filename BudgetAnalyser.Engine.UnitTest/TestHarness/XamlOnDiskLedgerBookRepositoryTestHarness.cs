using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.UnitTest.Helper;
using JetBrains.Annotations;
using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Engine.UnitTest.TestHarness
{
    internal class XamlOnDiskLedgerBookRepositoryTestHarness : XamlOnDiskLedgerBookRepository
    {
        public XamlOnDiskLedgerBookRepositoryTestHarness(
            [NotNull] IDtoMapper<LedgerBookDto, LedgerBook> mapper,
            IReaderWriterSelector selector) : base(mapper, new BankImportUtilitiesTestHarness(), new LedgerBookFactory(new ReconciliationBuilder(new FakeLogger())), selector)
        {
            LoadXamlFromDiskFromEmbeddedResources = true;
        }

        public string SerialisedData { get; private set; }
        public event EventHandler DtoDeserialised;
        public Func<string, bool> FileExistsOverride { get; set; }
        public LedgerBookDto LedgerBookDto { get; private set; }
        public Func<string, string> LoadXamlAsStringOverride { get; set; }
        public bool LoadXamlFromDiskFromEmbeddedResources { get; set; }
        public Action<LedgerBookDto> SaveDtoToDiskOverride { get; set; }

        protected override string LoadXamlAsString(string fileName)
        {
            if (LoadXamlAsStringOverride == null)
            {
                string result = base.LoadXamlAsString(fileName);
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
                EventHandler handler = DtoDeserialised;
                handler?.Invoke(this, EventArgs.Empty);

                return LedgerBookDto;
            }

            LedgerBookDto = await base.LoadXamlFromDiskAsync(fileName, isEncrypted);
            return LedgerBookDto;
        }

        protected override async Task SaveDtoToDiskAsync(LedgerBookDto dataEntity, bool isEncrypted)
        {
            if (SaveDtoToDiskOverride == null)
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
}