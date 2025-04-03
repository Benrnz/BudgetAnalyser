namespace BudgetAnalyser.Engine.Services;

[AutoRegisterWithIoC(SingleInstance = true)]
internal class DirtyDataService : IDirtyDataService
{
    private readonly Dictionary<ApplicationDataType, bool> dirtyData = new();
    private readonly ILogger logger;
    private readonly IMonitorableDependencies monitorableDependencies;

    public DirtyDataService(ILogger logger, IMonitorableDependencies monitorableDependencies)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.monitorableDependencies = monitorableDependencies ?? throw new ArgumentNullException(nameof(monitorableDependencies));
        InitialiseDirtyDataTable();
    }

    public void NotifyOfChange(ApplicationDataType dataType)
    {
        this.logger.LogInfo(_ => $"Data type {dataType} has been marked as DIRTY.");
        this.dirtyData[dataType] = true;
        this.monitorableDependencies.NotifyOfDependencyChange<IDirtyDataService>(this);
    }

    public bool IsDirty(ApplicationDataType dataType)
    {
        return this.dirtyData.GetValueOrDefault(dataType, false);
    }

    /// <inheritdoc />
    public bool HasUnsavedChanges => this.dirtyData.Values.Any(v => v);

    public void ClearAllDirtyDataFlags()
    {
        foreach (var key in this.dirtyData.Keys.ToList())
        {
            this.dirtyData[key] = false;
        }

        this.monitorableDependencies.NotifyOfDependencyChange<IDirtyDataService>(this);
        this.logger.LogInfo(_ => "All dirty data flags have been cleared.");
    }

    public void SetAllDirtyFlags()
    {
        // Ensure all data types are marked as requiring a save.
        foreach (var dataType in Enum.GetValues(typeof(ApplicationDataType)))
        {
            this.dirtyData[(ApplicationDataType)dataType] = true;
        }

        this.logger.LogInfo(_ => "All dirty data flags have been set as DIRTY.");
    }

    private void InitialiseDirtyDataTable()
    {
        foreach (int value in Enum.GetValues(typeof(ApplicationDataType)))
        {
            var enumValue = (ApplicationDataType)value;
            this.dirtyData.Add(enumValue, false);
        }
    }
}
