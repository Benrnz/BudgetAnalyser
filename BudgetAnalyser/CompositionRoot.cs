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
///     Composition root helper methods for service registration and object graph bootstrapping.
/// </summary>
public static class CompositionRoot
{
    /// <summary>
    ///     Register all application services with the given service collection.
    /// </summary>
    public static void ConfigureServices(IServiceCollection services)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        var encryptionAssembly = typeof(IFileEncryptor).GetTypeInfo().Assembly;
        var thisAssembly = typeof(CompositionRoot).GetTypeInfo().Assembly;

        // Auto-register all attributed types from each assembly.
        services.AddEngineRegistrations();
        services.AddAutoRegistrations(encryptionAssembly);
        services.AddAutoRegistrations(thisAssembly);

        // Registrations from Rees.Wpf - there are no automatic registrations in this assembly.
        RegisterReesWpf(services);

        // Override / supplement with explicit non-automatic registrations.
        RegisterNonAutomaticServices(services);

        // Root window is resolved from DI once startup is complete.
        services.AddSingleton<ShellWindow>();
    }

    /// <summary>
    ///     Build and initialise late-bound parts of the object graph that require a built provider.
    /// </summary>
    public static void BuildApplicationObjectGraph(IServiceProvider provider)
    {
        if (provider is null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        Debug.Assert(IsMainThread(), "CompositionRoot.BuildApplicationObjectGraph must be called on the main UI thread.");

        var engineAssembly = typeof(TransactionsListModel).GetTypeInfo().Assembly;
        var encryptionAssembly = typeof(IFileEncryptor).GetTypeInfo().Assembly;
        var thisAssembly = typeof(CompositionRoot).GetTypeInfo().Assembly;

        // Perform property injection for static classes that need it.
        // Property injection is a last resort, used only where data binding to static properties requires it.
        foreach (var assembly in new[] { engineAssembly, thisAssembly, encryptionAssembly })
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

        ConstructUiContext(provider);
    }

    /// <summary>
    ///     Build the <see cref="IUiContext" /> instance that is used by all <see cref="ControllerBase" /> controllers.
    ///     It contains references to commonly used UI components that most controllers require as well as references to all
    ///     other controllers.  All controllers are single instances.
    /// </summary>
    /// <param name="provider">The built service provider used to resolve controller instances.</param>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Composition Root Pattern - pull all abstract wiring into one place")]
    private static void ConstructUiContext(IServiceProvider provider)
    {
        var uiContext = provider.GetRequiredService<IUiContext>();
        Type[] controllerTypes =
        [
            typeof(AddLedgerReconciliationController),
            typeof(AppliedRulesController),
            typeof(ChooseBudgetBucketController),
            typeof(CreateNewFixedBudgetController),
            typeof(CreateNewSurprisePaymentMonitorController),
            typeof(DisusedRulesController),
            typeof(EditingTransactionController),
            typeof(EncryptFileController),
            typeof(GlobalFilterController),
            typeof(LedgerBucketViewController),
            typeof(LedgerRemarksController),
            typeof(LedgerTransactionsController),
            typeof(MainMenuController),
            typeof(NewBudgetModelController),
            typeof(NewRuleController),
            typeof(OverallPerformanceController),
            typeof(ReconciliationToDoListController),
            typeof(ShowSurplusBalancesController),
            typeof(SplitTransactionController),
            typeof(TopBudgetController),
            typeof(TopDashboardController),
            typeof(TopLedgerBookController),
            typeof(TopReportsCatalogController),
            typeof(TopRulesController),
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

    private static bool IsMainThread()
    {
        return Application.Current.Dispatcher.CheckAccess();
    }

    /// <summary>
    ///     Register any special mappings that have not been registered with automatic mappings.
    ///     Explicit registrations here override any auto-registered counterparts for the same service type.
    /// </summary>
    private static void RegisterNonAutomaticServices(IServiceCollection services)
    {
        // ILogger: explicit singleton so log level can be configured once at startup.
        services.AddSingleton<ILogger, DebugLogger>();

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
}
