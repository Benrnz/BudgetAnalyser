using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.Engine.Widgets.Data;
using Rees.TangyFruitMapper;
using Xunit.Abstractions;

namespace BudgetAnalyser.Engine.XUnit.TestHarness;

internal class XamlOnDiskWidgetRepositoryTestHarness(IDtoMapper<WidgetDto, Widget> mapper, IReaderWriterSelector selector, ITestOutputHelper output)
    : XamlOnDiskWidgetRepository(mapper, new XUnitLogger(output), selector)
{
    public Func<string, bool> ExistsOveride { get; set; }
    public Func<string, List<WidgetDto>> LoadFromDiskOveride { get; set; }
    public Action<string, IEnumerable<WidgetDto>> SaveToDiskOveride { get; set; }
    public string SerialisedData { get; set; }

    protected override async Task<List<WidgetDto>> LoadFromDiskAsync(string fileName, bool isEncrypted)
    {
        return LoadFromDiskOveride is null ? await base.LoadFromDiskAsync(fileName, isEncrypted) : LoadFromDiskOveride(fileName);
    }

    protected override async Task SaveToDiskAsync(string fileName, IEnumerable<WidgetDto> dataEntities, bool isEncrypted)
    {
        if (SaveToDiskOveride is null)
        {
            await Task.CompletedTask;
            return;
        }

        await Task.Run(() => SaveToDiskOveride(fileName, dataEntities));
    }

    protected override string Serialise(IEnumerable<WidgetDto> dataEntity)
    {
        SerialisedData = base.Serialise(dataEntity);
        return SerialisedData;
    }
}
