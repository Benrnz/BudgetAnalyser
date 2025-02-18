using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Matching.Data;
using BudgetAnalyser.Engine.Persistence;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.UnitTest.TestHarness;

internal class XamlOnDiskMatchingRuleRepositoryTestHarness : XamlOnDiskMatchingRuleRepository
{
    public XamlOnDiskMatchingRuleRepositoryTestHarness([NotNull] IDtoMapper<MatchingRuleDto, MatchingRule> mapper, IReaderWriterSelector selector) : base(mapper, new FakeLogger(), selector)
    {
    }

    public Func<string, bool> ExistsOveride { get; set; }
    public Func<string, List<MatchingRuleDto>> LoadFromDiskOveride { get; set; }
    public Action<string, IEnumerable<MatchingRuleDto>> SaveToDiskOveride { get; set; }
    public string SerialisedData { get; set; }

    protected override async Task<List<MatchingRuleDto>> LoadFromDiskAsync(string fileName, bool isEncrypted)
    {
        return LoadFromDiskOveride is null ? await base.LoadFromDiskAsync(fileName, isEncrypted) : LoadFromDiskOveride(fileName);
    }

    protected override async Task SaveToDiskAsync(string fileName, IEnumerable<MatchingRuleDto> dataEntities, bool isEncrypted)
    {
        if (SaveToDiskOveride is null)
        {
            await Task.Delay(1);
            return;
        }

        await Task.Run(() => SaveToDiskOveride(fileName, dataEntities));
    }

    protected override string Serialise(IEnumerable<MatchingRuleDto> dataEntity)
    {
        SerialisedData = base.Serialise(dataEntity);
        return SerialisedData;
    }
}
