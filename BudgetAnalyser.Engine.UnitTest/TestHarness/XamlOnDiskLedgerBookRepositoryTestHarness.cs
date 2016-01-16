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
            [NotNull] IDtoMapper<LedgerBookDto, LedgerBook> mapper) : base(mapper, new BankImportUtilitiesTestHarness(), new LedgerBookFactory(new ReconciliationBuilder(new FakeLogger())))
        {
            LoadXamlFromDiskFromEmbeddedResources = true;
        }

        public event EventHandler DtoDeserialised;
        public Func<string, bool> FileExistsOverride { get; set; }
        public LedgerBookDto LedgerBookDto { get; private set; }
        public Func<string, string> LoadXamlAsStringOverride { get; set; }
        public bool LoadXamlFromDiskFromEmbeddedResources { get; set; }
        public Action<LedgerBookDto> SaveDtoToDiskOverride { get; set; }
        public Action<string, string> WriteToDiskOverride { get; set; }

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

        protected override async Task<LedgerBookDto> LoadXamlFromDiskAsync(string fileName)
        {
            if (LoadXamlFromDiskFromEmbeddedResources)
            {
                LedgerBookDto = EmbeddedResourceHelper.ExtractXaml<LedgerBookDto>(fileName, true);
                EventHandler handler = DtoDeserialised;
                handler?.Invoke(this, EventArgs.Empty);

                return LedgerBookDto;
            }

            LedgerBookDto = await base.LoadXamlFromDiskAsync(fileName);
            return LedgerBookDto;
        }

        protected override async Task SaveDtoToDiskAsync(LedgerBookDto dataEntity)
        {
            if (SaveDtoToDiskOverride == null)
            {
                await base.SaveDtoToDiskAsync(dataEntity);
                return;
            }

            SaveDtoToDiskOverride(dataEntity);
        }

        protected override Task WriteToDiskAsync(string filename, string data)
        {
            return Task.Run(() => WriteToDiskOverride(filename, data));
        }
    }
}