using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Windows;
using BudgetAnalyser.ApplicationState;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Encryption;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Persistence;
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

namespace BudgetAnalyser;

/// <summary>
///     Composition root helper methods for service registration and object graph bootstrapping.
/// </summary>
public static class CompositionHelper
{
    private static bool ControllersInitialised;

    /// <summary>
    ///     Build and initialise late-bound parts of the object graph that require a built provider.
    /// </summary>
    public static void BuildApplicationObjectGraph(IServiceProvider provider)
    {
        if (provider is null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        var isMainThread = Application.Current.Dispatcher.CheckAccess();
        Debug.Assert(isMainThread, "CompositionHelper.BuildApplicationObjectGraph must be called on the main UI thread.");

        var engineAssembly = typeof(TransactionsListModel).GetTypeInfo().Assembly;
        var encryptionAssembly = typeof(IFileEncryptor).GetTypeInfo().Assembly;
        var thisAssembly = typeof(CompositionHelper).GetTypeInfo().Assembly;

        // Perform property injection for static classes that need it.
        // Property injection is a last resort, used only where data binding to static properties requires it.
        foreach (var assembly in new[] { engineAssembly, thisAssembly, encryptionAssembly })
        {
            var requiredPropertyInjections = EngineIocRegistrations.ProcessPropertyInjection(assembly);
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
    ///     Perform one-time controller initialisation and application state rehydration.  Any Controller that implements <see cref="IInitializableController" /> will have its
    ///     <see cref="IInitializableController.Initialize" /> method called.
    /// </summary>
    public static void InitialiseControllers(ILogger logger, IPersistApplicationState statePersistence, IMessenger messenger, IEnumerable<IInitializableController> initializableControllers)
    {
        if (ControllersInitialised)
        {
            return;
        }

        logger.LogInfo(_ => $"ShellController Initialise started. {DateTime.Now}");

        ControllersInitialised = true;

        IList<IPersistentApplicationStateObject> rehydratedState = statePersistence.Load().ToList();
        if (rehydratedState.None())
        {
            rehydratedState = CreateNewDefaultApplicationState();
        }

        // Create a distinct list of sequences.
        var sequences = rehydratedState.Select(persistentModel => persistentModel.LoadSequence).OrderBy(s => s).Distinct();

        logger.LogInfo(_ => $"Calling Initialise on each Controller. {DateTime.Now}");
        initializableControllers.ToList().ForEach(i =>
        {
            i.Initialize();
            logger.LogInfo(_ => $"Initialise called on Controller: {i.GetType().Name}");
        });

        // Send state load messages in order.
        foreach (var sequence in sequences)
        {
            var sequenceCopy = sequence;
            var models = rehydratedState.Where(persistentModel => persistentModel.LoadSequence == sequenceCopy);
            logger.LogInfo(_ => $"ShellController sending ApplicationStateLoadedMessage for: Sequence{sequence} {models.First().GetType().Name}");
            messenger.Send(new ApplicationStateLoadedMessage(models));
        }

        logger.LogInfo(_ => $"ShellController Initialise completing. Sending ApplicationStateLoadFinishedMessage. {DateTime.Now}");
        messenger.Send(new ApplicationStateLoadFinishedMessage());
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

    private static IList<IPersistentApplicationStateObject> CreateNewDefaultApplicationState()
    {
        var appState = new List<IPersistentApplicationStateObject>();
        return appState;
    }
}
