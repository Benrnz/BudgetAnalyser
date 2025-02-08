using BudgetAnalyser.Engine.Widgets.Data;
using Portable.Xaml;
using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     A Repository to persistently store widgets in Xaml format on local disk.
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
internal class XamlOnDiskWidgetRepository(IDtoMapper<WidgetDto, Widget> mapper, ILogger logger, IReaderWriterSelector readerWriterSelector, IStandardWidgetCatalog catalog) : IWidgetRepository
{
    private readonly IStandardWidgetCatalog catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IDtoMapper<WidgetDto, Widget> mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IReaderWriterSelector readerWriterSelector = readerWriterSelector ?? throw new ArgumentNullException(nameof(readerWriterSelector));

    public async Task CreateNewAndSaveAsync(string storageKey)
    {
        if (storageKey.IsNothing())
        {
            throw new ArgumentNullException(nameof(storageKey));
        }

        await SaveAsync(CreateNewUsingDefaultSetOfWidgets(), storageKey, false);
    }

    public async Task<IEnumerable<Widget>> LoadAsync(string storageKey, bool isEncrypted)
    {
        this.logger.LogInfo(_ => $"{nameof(XamlOnDiskWidgetRepository)} Loading Widgets from: {storageKey}");
        if (storageKey.IsNothing())
        {
            this.logger.LogWarning(_ => $"{nameof(XamlOnDiskWidgetRepository)} Storage key is empty: {storageKey}");
            throw new KeyNotFoundException("storageKey is blank");
        }

        var reader = this.readerWriterSelector.SelectReaderWriter(isEncrypted);
        if (!reader.FileExists(storageKey))
        {
            this.logger.LogWarning(_ => $"{nameof(XamlOnDiskWidgetRepository)} Storage key cannot be found: {storageKey}");
            throw new KeyNotFoundException("Storage key can not be found: " + storageKey);
        }

        List<WidgetDto> dataEntities;
        try
        {
            dataEntities = await LoadFromDiskAsync(storageKey, isEncrypted);
        }
        catch (Exception ex)
        {
            this.logger.LogWarning(_ => $"{nameof(XamlOnDiskWidgetRepository)} Deserialisation failed for: {storageKey}");
            throw new DataFormatException("Deserialisation Widgets failed, an exception was thrown by the Xaml deserialiser, the file format is invalid.", ex);
        }

        if (dataEntities is null)
        {
            this.logger.LogWarning(_ => $"{nameof(XamlOnDiskWidgetRepository)} Deserialised widget file completed but isn't castable into List<WidgetDto>");
            throw new DataFormatException("Deserialised Widgets are not of type List<WidgetDto>");
        }

        var realModel = dataEntities.Select(d => this.mapper.ToModel(d));
        this.logger.LogInfo(_ => $"{nameof(XamlOnDiskWidgetRepository)} Loaded {realModel.Count()} widgets from: {storageKey}");
        return realModel.ToList();
    }

    public async Task SaveAsync(IEnumerable<Widget> widgets, string storageKey, bool isEncrypted)
    {
        if (widgets is null)
        {
            throw new ArgumentNullException(nameof(widgets));
        }

        if (storageKey is null)
        {
            throw new ArgumentNullException(nameof(storageKey));
        }

        this.logger.LogInfo(_ => $"{nameof(XamlOnDiskWidgetRepository)} Saving Widgets to: {storageKey}");
        var dataEntities = widgets.Select(r => this.mapper.ToDto(r));
        await SaveToDiskAsync(storageKey, dataEntities, isEncrypted);
        this.logger.LogInfo(_ => $"{nameof(XamlOnDiskWidgetRepository)} Saved Widgets to: {storageKey}");
    }

    private IEnumerable<Widget> CreateNewUsingDefaultSetOfWidgets()
    {
        return this.catalog.GetAll();
    }

    private object Deserialise(string xaml)
    {
        return XamlServices.Parse(xaml);
    }

    private async Task<List<WidgetDto>> LoadFromDiskAsync(string fileName, bool isEncrypted)
    {
        var reader = this.readerWriterSelector.SelectReaderWriter(isEncrypted);
        var result = await reader.LoadFromDiskAsync(fileName);
        return (List<WidgetDto>)Deserialise(result);
    }

    private async Task SaveToDiskAsync(string fileName, IEnumerable<WidgetDto> dataEntities, bool isEncrypted)
    {
        var writer = this.readerWriterSelector.SelectReaderWriter(isEncrypted);
        await writer.WriteToDiskAsync(fileName, Serialise(dataEntities));
    }

    private string Serialise(IEnumerable<WidgetDto> dataEntities)
    {
        return dataEntities is null ? throw new ArgumentNullException(nameof(dataEntities)) : XamlServices.Save(dataEntities.ToList());
    }
}
