using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Windows;
using BudgetAnalyser.ApplicationState;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Encryption;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.Matching;
using BudgetAnalyser.Mobile;
using BudgetAnalyser.ReportsCatalog;
using BudgetAnalyser.ReportsCatalog.OverallPerformance;
using BudgetAnalyser.Transactions;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Rees.Wpf;
using Rees.Wpf.Contracts;
using Rees.Wpf.UserInteraction;
using IPersistApplicationState = BudgetAnalyser.ApplicationState.IPersistApplicationState;

namespace BudgetAnalyser;

/// <summary>
///     This class follows the Composition Root Pattern as described here: http://blog.ploeh.dk/2011/07/28/CompositionRoot/. It constructs any and all required objects, the whole graph for use for
///     the process lifetime of the application. This class also contains all usage of IoC to this class.  It should not be used anywhere else in the application to prevent
///     the Service Locator anti-pattern from appearing.
/// </summary>
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Reviewed, ok here: Necessary for composition root pattern")]
public sealed class CompositionRoot : IDisposable
{
    private const string InputBoxView = "InputBoxView";
    private readonly List<IDisposable> disposables = new();
    private bool disposed;

    public CompositionRoot()
    {
        Debug.Assert(IsMainThread(), "CompositionRoot.Compose must be called with the Main UI Thread.");
        var services = new ServiceCollection();
        var engineAssembly = typeof(TransactionsListModel).GetTypeInfo().Assembly;
        var storageAssembly = typeof(IFileEncryptor).GetTypeInfo().Assembly;
        var thisAssembly = GetType().GetTypeInfo().Assembly;

        var stopwatch = Stopwatch.StartNew();

        // Auto-register all attributed types from each assembly.
        services.AddEngineRegistrations();
        services.AddAutoRegistrations(storageAssembly);
        services.AddAutoRegistrations(thisAssembly);

        // Registrations from Rees.Wpf - there are no automatic registrations in this assembly.
        RegisterReesWpf(services);

        // Override / supplement with explicit non-automatic registrations.
        RegisterNonAutomaticServices(services);

        var timeTakenToRegister = stopwatch.ElapsedMilliseconds;

        stopwatch = Stopwatch.StartNew();
        var provider = services.BuildServiceProvider();

        Logger = provider.GetRequiredService<ILogger>();
        Logger.LogLevelFilter = LogLevel.Info; // hardcoded default log level.

        BuildApplicationObjectGraph(provider, engineAssembly, thisAssembly, storageAssembly);
        ShellController = provider.GetRequiredService<ShellController>();
        stopwatch.Stop();
        var timeTakenToBuildObjectGraph = stopwatch.ElapsedMilliseconds;
        Logger.LogAlways(_ => $"Enumerating all types and registering types took: {timeTakenToRegister}ms. Building the object graph took: {timeTakenToBuildObjectGraph}ms.");

        this.disposables.AddIfSomething(provider);
    }

    /// <summary>
    ///     The newly instantiated global logger ready for use.  Prior to executing the composition root the logger will not been available.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    ///     The top level Controller / ViewModel for the top level window aka <see cref="ShellWindow" />.  This is the first object to be called following execution of this Composition Root.
    /// </summary>
    public ShellController ShellController { get; }

    /// <summary>
    ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        // This method is only called by the Main Thread, and does not need to be thread safe.
        if (!this.disposed)
        {
            this.disposables.ForEach(x => x.Dispose());
        }

        this.disposed = true;

        // Take this object off the Finalization queue to prevent finalization code for this object from executing a second time.
        GC.SuppressFinalize(this);
    }

    private void BuildApplicationObjectGraph(IServiceProvider provider, params Assembly[] assemblies)
    {
        // Perform property injection for static classes that need it.
        // Property injection is a last resort, used only where data binding to static properties requires it.
        foreach (var assembly in assemblies)
        {
            var requiredPropertyInjections = DefaultIoCRegistrations.ProcessPropertyInjection(assembly);
            foreach (var requirement in requiredPropertyInjections)
            {
                var dependency = provider.GetService(requirement.Type);
                if (dependency is not null)
                {
                    requirement.PropertyInjectionAssignment(dependency);
                }
            }
        }

        // Kick it off
        ConstructUiContext(provider);
    }

    /// <summary>
    ///     Build the <see cref="IUiContext" /> instance that is used by all <see cref="ControllerBase" /> controllers.
    ///     It contains references to commonly used UI components that most controllers require as well as references to all
    ///     other controllers.  All controllers are single instances.
    /// </summary>
    /// <param name="provider">The built service provider used to resolve controller instances.</param>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Composition Root Pattern - pull all abstract wiring into one place")]
    private void ConstructUiContext(IServiceProvider provider)
    {
        var uiContext = provider.GetRequiredService<IUiContext>();
        Type[] controllerTypes =
        [
            typeof(AddLedgerReconciliationController),
            typeof(AppliedRulesController),
            typeof(TopBudgetController),
            typeof(ChooseBudgetBucketController),
            typeof(CreateNewFixedBudgetController),
            typeof(CreateNewSurprisePaymentMonitorController),
            typeof(TopDashboardController),
            typeof(DisusedRulesController),
            typeof(EditingTransactionController),
            typeof(EncryptFileController),
            typeof(GlobalFilterController),
            typeof(TopLedgerBookController),
            typeof(LedgerBucketViewController),
            typeof(LedgerRemarksController),
            typeof(LedgerTransactionsController),
            typeof(MainMenuController),
            typeof(NewBudgetModelController),
            typeof(NewRuleController),
            typeof(OverallPerformanceController),
            typeof(ReconciliationToDoListController),
            typeof(TopReportsCatalogController),
            typeof(TopRulesController),
            typeof(ShowSurplusBalancesController),
            typeof(SplitTransactionController),
            typeof(TopTransactionsListController),
            typeof(TransferFundsController),
            typeof(UploadMobileDataController)
        ];

        var controllers = new Dictionary<Type, Lazy<ControllerBase>>();
        foreach (var controllerType in controllerTypes)
        {
            var lazy = new Lazy<ControllerBase>(() => (ControllerBase)provider.GetRequiredService(controllerType));
            controllers.Add(controllerType, lazy);
        }

        uiContext.Initialise(controllers);
    }

    private bool IsMainThread()
    {
        return Application.Current.Dispatcher.CheckAccess();
    }

    /// <summary>
    ///     Register any special mappings that have not been registered with automatic mappings.
    ///     Explicit registrations here override any auto-registered counterparts for the same service type.
    /// </summary>
    private static void RegisterNonAutomaticServices(IServiceCollection services)
    {
        // ILogger: explicit transient so the log level can be configured after resolution.
        services.AddTransient<ILogger, DebugLogger>();

        // IUiContext: singleton ambient context used by all controllers.
        services.AddSingleton<IUiContext, UiContext>();

        // IMessenger: supply WeakReferenceMessenger.Default as the inner messenger so that
        // controllers which rely on the default messenger in their base class share the same bus.
        // NOTE: this must be the only place WeakReferenceMessenger.Default is referenced.
        // A factory is used here rather than auto-registration to avoid a circular dependency
        // (ConcurrentMessenger itself takes IMessenger in its constructor).
        services.AddSingleton<IMessenger>(sp => new ConcurrentMessenger(WeakReferenceMessenger.Default, sp.GetRequiredService<ILogger>()));
    }

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Required here, Composition Root pattern")]
    private static void RegisterReesWpf(IServiceCollection services)
    {
        // Wait cursor factory.
        services.AddSingleton<Func<IWaitCursor>>(() => new WpfWaitCursor());

        services.AddSingleton<IPersistApplicationState, PersistBaxAppStateAsJson>();

        // Input box: resolved by concrete type so that WindowsInputBox can receive the right IViewLoader.
        services.AddTransient<WpfViewLoader<InputBox>>();
        services.AddTransient<IUserInputBox>(sp => new WindowsInputBox(sp.GetRequiredService<WpfViewLoader<InputBox>>()));

        services.AddSingleton<IUserMessageBox, WindowsMessageBox>();
        services.AddSingleton<IUserQuestionBoxYesNo, WindowsQuestionBoxYesNo>();

        services.AddSingleton<Func<IUserPromptOpenFile>>(_ => () => new WindowsOpenFileDialog { AddExtension = true, CheckFileExists = true, CheckPathExists = true });
        services.AddSingleton<Func<IUserPromptSaveFile>>(_ => () => new WindowsSaveFileDialog { AddExtension = true, CheckPathExists = true });
    }

    /// <summary>
    ///     Finalizes an instance of the <see cref="CompositionRoot" /> class.
    ///     Use C# destructor syntax for finalization code.
    ///     This destructor will run only if the Dispose method
    ///     does not get called.
    ///     It gives your base class the opportunity to finalize.
    ///     Do not provide destructors in types derived from this class.
    /// </summary>
    ~CompositionRoot()
    {
        // Do not re-create Dispose clean-up code here.
        // Calling Dispose(false) is optimal in terms of
        // readability and maintainability.
        Dispose();
    }
}
