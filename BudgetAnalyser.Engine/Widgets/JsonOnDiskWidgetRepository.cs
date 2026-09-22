using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Widgets.Data;

namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     A Repository to persistently store widgets in Json format on local disk.
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
internal class JsonOnDiskWidgetRepository(IDtoMapper<WidgetDto, Widget> mapper, ILogger logger, IReaderWriterSelector readerWriterSelector, IStandardWidgetCatalog catalog)
    : JsonOnDiskRepositoryBase<List<WidgetDto>>(readerWriterSelector), IWidgetRepository
{
    private readonly IStandardWidgetCatalog catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IDtoMapper<WidgetDto, Widget> mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    /// <inheritdoc />
    public async Task CreateNewAndSaveAsync(string storageKey)
    {
        if (storageKey.IsNothing())
        {
            throw new ArgumentNullException(nameof(storageKey));
        }

        await SaveAsync(CreateNewUsingDefaultSetOfWidgets(), storageKey, false);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Widget>> LoadAsync(string storageKey, bool isEncrypted)
    {
        this.logger.LogInfo(_ => $"{nameof(JsonOnDiskWidgetRepository)} Loading Widgets from: {storageKey}");
        if (storageKey.IsNothing())
        {
            this.logger.LogWarning(_ => $"{nameof(JsonOnDiskWidgetRepository)} Storage key is empty: {storageKey}");
            throw new KeyNotFoundException("storageKey is blank");
        }

        var reader = ReaderWriterSelector.SelectReaderWriter(isEncrypted);
        if (!reader.FileExists(storageKey))
        {
            this.logger.LogWarning(_ => $"{nameof(JsonOnDiskWidgetRepository)} Storage key cannot be found: {storageKey}");
            throw new KeyNotFoundException("Storage key can not be found: " + storageKey);
        }

        List<WidgetDto> dataEntities;
        try
        {
            dataEntities = await LoadJsonFromDiskAsync(storageKey, isEncrypted);
        }
        catch (Exception ex)
        {
            this.logger.LogWarning(_ => $"{nameof(JsonOnDiskWidgetRepository)} Deserialisation failed for: {storageKey}");
            throw new DataFormatException("Deserialisation Widgets failed, an exception was thrown by the Json deserialiser, the file format is invalid.", ex);
        }

        var realModel = dataEntities.Select(d => this.mapper.ToModel(d));
        this.logger.LogInfo(_ => $"{nameof(JsonOnDiskWidgetRepository)} Loaded {realModel.Count()} widgets from: {storageKey}");
        return realModel.ToList();
    }

    /// <inheritdoc />
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

        this.logger.LogInfo(_ => $"{nameof(JsonOnDiskWidgetRepository)} Saving Widgets to: {storageKey}");
        var dataEntities = MapToDto(widgets);
        await SaveToDiskAsync(storageKey, dataEntities, isEncrypted);
        this.logger.LogInfo(_ => $"{nameof(JsonOnDiskWidgetRepository)} Saved Widgets to: {storageKey}");
    }

    protected override Exception CreateCorruptFileException()
    {
        return new DataFormatException("Unable to deserialise Widgets into the correct type. File is corrupt.");
    }

    protected virtual List<WidgetDto> MapToDto(IEnumerable<Widget> widgets)
    {
        return widgets.Select(r => this.mapper.ToDto(r)).ToList();
    }

    private IEnumerable<Widget> CreateNewUsingDefaultSetOfWidgets()
    {
        return this.catalog.GetAll();
    }
}
