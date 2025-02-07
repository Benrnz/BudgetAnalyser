using System.Reflection;
using BudgetAnalyser.Engine.Widgets.Data;
using Portable.Xaml;
using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     A Repository to persistently store widgets in Xaml format on local disk.
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
internal class XamlOnDiskWidgetRepository(IDtoMapper<WidgetDto, Widget> mapper, ILogger logger, IReaderWriterSelector readerWriterSelector)
{
    private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IDtoMapper<WidgetDto, Widget> mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IReaderWriterSelector readerWriterSelector = readerWriterSelector ?? throw new ArgumentNullException(nameof(readerWriterSelector));

    public IEnumerable<Widget> CreateNew()
    {
        var widgetTypes = GetType().GetTypeInfo().Assembly.GetExportedTypes()
            .Where(t => typeof(Widget).IsAssignableFrom(t) && !t.GetTypeInfo().IsAbstract);

        var widgets = widgetTypes
            .Where(t => !typeof(IUserDefinedWidget).IsAssignableFrom(t))
            .Select(widgetType => Activator.CreateInstance(widgetType) as Widget)
            .ToList();

        if (widgets.Any(w => w is null))
        {
            throw new DataFormatException("Code Error: Widget could not be created.");
        }

        return widgets.Where(w => w is not null)
            .Cast<Widget>()
            .ToList();
    }

    public async Task CreateNewAndSaveAsync(string storageKey)
    {
        if (storageKey.IsNothing())
        {
            throw new ArgumentNullException(nameof(storageKey));
        }

        await SaveAsync(new List<Widget>(), storageKey, false);
    }

    public async Task<IEnumerable<Widget>> LoadAsync(string storageKey, bool isEncrypted)
    {
        if (storageKey.IsNothing())
        {
            throw new KeyNotFoundException("storageKey is blank");
        }

        var reader = this.readerWriterSelector.SelectReaderWriter(isEncrypted);
        if (!reader.FileExists(storageKey))
        {
            throw new KeyNotFoundException("Storage key can not be found: " + storageKey);
        }

        List<WidgetDto> dataEntities;
        try
        {
            dataEntities = await LoadFromDiskAsync(storageKey, isEncrypted);
        }
        catch (Exception ex)
        {
            throw new DataFormatException("Deserialisation Widgets failed, an exception was thrown by the Xaml deserialiser, the file format is invalid.", ex);
        }

        if (dataEntities is null)
        {
            throw new DataFormatException("Deserialised Widgets are not of type List<WidgetDto>");
        }

        var realModel = dataEntities.Select(d => this.mapper.ToModel(d));
        return realModel.ToList();
    }

    public async Task SaveAsync(IEnumerable<Widget> rules, string storageKey, bool isEncrypted)
    {
        if (rules is null)
        {
            throw new ArgumentNullException(nameof(rules));
        }

        if (storageKey is null)
        {
            throw new ArgumentNullException(nameof(storageKey));
        }

        var dataEntities = rules.Select(r => this.mapper.ToDto(r));
        await SaveToDiskAsync(storageKey, dataEntities, isEncrypted);
    }

    // ReSharper disable once VirtualMemberNeverOverridden.Global // Used in unit testing
    protected virtual object Deserialise(string xaml)
    {
        return XamlServices.Parse(xaml);
    }

    protected virtual async Task<List<WidgetDto>> LoadFromDiskAsync(string fileName, bool isEncrypted)
    {
        var reader = this.readerWriterSelector.SelectReaderWriter(isEncrypted);
        var result = await reader.LoadFromDiskAsync(fileName);
        return (List<WidgetDto>)Deserialise(result);
    }

    protected virtual async Task SaveToDiskAsync(string fileName, IEnumerable<WidgetDto> dataEntities, bool isEncrypted)
    {
        var writer = this.readerWriterSelector.SelectReaderWriter(isEncrypted);
        await writer.WriteToDiskAsync(fileName, Serialise(dataEntities));
    }

    protected virtual string Serialise(IEnumerable<WidgetDto> dataEntity)
    {
        return dataEntity is null ? throw new ArgumentNullException(nameof(dataEntity)) : XamlServices.Save(dataEntity.ToList());
    }
}
