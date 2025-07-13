using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Windows;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using BudgetAnalyser.ApplicationState;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Encryption;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.Matching;
using BudgetAnalyser.Mobile;
using BudgetAnalyser.ReportsCatalog;
using BudgetAnalyser.ReportsCatalog.OverallPerformance;
using BudgetAnalyser.Statement;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;
using Rees.Wpf.Contracts;
using Rees.Wpf.UserInteraction;
using IPersistApplicationState = BudgetAnalyser.ApplicationState.IPersistApplicationState;

namespace BudgetAnalyser;

/// <summary>
///     This class follows the Composition Root Pattern as described here: http://blog.ploeh.dk/2011/07/28/CompositionRoot/. It constructs any and all required objects, the whole graph for use for
///     the process lifetime of the application. This class also contains all usage of IoC (Autofac in this case) to this class.  It should not be used anywhere else in the application to prevent
///     the Service Locator anti-pattern from appearing.
/// </summary>
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Reviewed, ok here: Necessary for composition root pattern")]
public sealed class CompositionRoot : IDisposable
{
    private const string InputBoxView = "InputBoxView";
    // TODO Won't be needed under AppHost and Ms DI.
    private readonly List<IDisposable> disposables = new();
    private bool disposed;

    public CompositionRoot()
    {
        Debug.Assert(IsMainThread(), "CompositionRoot.Compose must be called with the Main UI Thread.");
        var builder = new ContainerBuilder();
        var engineAssembly = typeof(TransactionSetModel).GetTypeInfo().Assembly;
        var storageAssembly = typeof(IFileEncryptor).GetTypeInfo().Assembly;
        var thisAssembly = GetType().GetTypeInfo().Assembly;

        var stopwatch = Stopwatch.StartNew();
        builder.RegisterAssemblyTypes(thisAssembly).AsSelf();

        ComposeTypesWithDefaultImplementations(storageAssembly, builder);
        ComposeTypesWithDefaultImplementations(engineAssembly, builder);
        ComposeTypesWithDefaultImplementations(thisAssembly, builder);

        // Register Messenger Singleton from MVVM CommunityToolkit
        // NOTE This should be the only place we refer to WeakReferenceMessenger.Default
        // Choosing to customise this could result in two messengers being used in the application. Controllers currently rely on the default messenger set in the base class, not this one.
        builder.RegisterType<ConcurrentMessenger>().As<IMessenger>().SingleInstance().WithParameter("defaultMessenger", WeakReferenceMessenger.Default);

        // Registrations from Rees.Wpf - There are no automatic registrations in this assembly.
        RegistrationsForReesWpf(builder);

        AllLocalNonAutomaticRegistrations(builder);
        var timeTakenToRegister = stopwatch.ElapsedMilliseconds;
        stopwatch = Stopwatch.StartNew();

        var container = builder.Build();

        Logger = container.Resolve<ILogger>();
        Logger.LogLevelFilter = LogLevel.Info; // hardcoded default log level.

        BuildApplicationObjectGraph(container, engineAssembly, thisAssembly, storageAssembly);
        ShellController = container.Resolve<ShellController>();
        stopwatch.Stop();
        var timeTakenToBuildObjectGraph = stopwatch.ElapsedMilliseconds;
        Logger.LogAlways(_ => $"Enumerating all types and registering types took: {timeTakenToRegister}ms. Building the object graph took: {timeTakenToBuildObjectGraph}ms.");
    }

    /// <summary>
    ///     The newly instantiated global logger ready for use.  Prior to executing the composition root the logger has not been available.
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
        // TODO Wont be needed under AppHost and Ms DI.
        // This method is only called by the Main Thread, and does not need to be thread safe.
        // Check to see if Dispose has already been called.
        if (!this.disposed)
        {
            // Release unmanaged resources. If disposing is false, only the following code is executed.
            this.disposables.ForEach(x => x.Dispose());
        }

        this.disposed = true;

        // Take this object off the Finalization queue to prevent finalization code for this object from executing a second time.
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Register any special mappings that have not been registered with automatic mappings. Explicit object creation below is necessary to correctly register with IoC container.
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "builder", Justification = "Good template code and likely use in the future")]
    private static void AllLocalNonAutomaticRegistrations(ContainerBuilder builder)
    {
        builder.RegisterType<DebugLogger>().As<ILogger>();
        builder.RegisterType<UiContext>().As<IUiContext>().SingleInstance();
    }

    private void BuildApplicationObjectGraph(IContainer container, params Assembly[] assemblies)
    {
        // Instantiate and store all controllers...
        // These must be executed in the order of dependency.  For example the RulesController requires a NewRuleController so the NewRuleController must be instantiated first.

        foreach (var assembly in assemblies)
        {
            var requiredPropertyInjections = DefaultIoCRegistrations.ProcessPropertyInjection(assembly);
            foreach (var requirement in requiredPropertyInjections)
            {
                // Some reasonably awkward Autofac usage here to allow testability.  (Extension methods aren't easy to test)
                var typedService = new TypedService(requirement.DependencyRequired);
                var success = container.ComponentRegistry.TryGetServiceRegistration(typedService, out var registration);
                if (success)
                {
                    var requestToResolve = new ResolveRequest(typedService, registration, []);
                    var dependency = container.ResolveComponent(requestToResolve);
                    requirement.PropertyInjectionAssignment(dependency);
                }
            }
        }

        // Kick it off
        ConstructUiContext(container);
        // TODO Don't need to do this if the AppHost is disposed.
        this.disposables.AddIfSomething(container.Resolve<ICredentialStore>() as IDisposable);
    }

    private static void ComposeTypesWithDefaultImplementations(Assembly assembly, ContainerBuilder builder)
    {
        var dependencies = DefaultIoCRegistrations.RegisterAutoMappingsFromAssembly(assembly);
        foreach (var dependency in dependencies)
        {
            IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> registration;
            if (dependency.IsSingleInstance)
            {
                // Singleton
                registration = builder.RegisterType(dependency.DependencyRequired).SingleInstance();
            }
            else
            {
                // Transient
                registration = builder.RegisterType(dependency.DependencyRequired).InstancePerDependency();
            }

            if (dependency.NamedInstanceName is not null)
            {
                // Named Dependency
                registration = registration.Named(dependency.NamedInstanceName, dependency.DependencyRequired);
            }

            registration.AsImplementedInterfaces().AsSelf();
        }
    }

    /// <summary>
    ///     Build the <see cref="IUiContext" /> instance that is used by all <see cref="ControllerBase" /> controllers.
    ///     It contains references to commonly used UI components that most controllers require as well as references to all
    ///     other controllers.  All controllers are single instances.
    /// </summary>
    /// <param name="container">The newly created (and short lived) IoC container used to instantiate objects.</param>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Composition Root Pattern - pull all abstract wiring into one place")]
    private void ConstructUiContext(IContainer container)
    {
        var uiContext = container.Resolve<IUiContext>();
        Type[] controllerTypes =
        [
            typeof(AddLedgerReconciliationController),
            typeof(AppliedRulesController),
            typeof(TabBudgetController),
            typeof(ChooseBudgetBucketController),
            typeof(CreateNewFixedBudgetController),
            typeof(CreateNewSurprisePaymentMonitorController),
            typeof(TabDashboardController),
            typeof(DisusedRulesController),
            typeof(EditingTransactionController),
            typeof(EncryptFileController),
            typeof(GlobalFilterController),
            typeof(TabLedgerBookController),
            typeof(LedgerBucketViewController),
            typeof(LedgerRemarksController),
            typeof(LedgerTransactionsController),
            typeof(MainMenuController),
            typeof(NewBudgetModelController),
            typeof(NewRuleController),
            typeof(OverallPerformanceController),
            typeof(ReconciliationToDoListController),
            typeof(TabReportsCatalogController),
            typeof(RulesController),
            typeof(ShowSurplusBalancesController),
            typeof(SplitTransactionController),
            typeof(TabTransactionsController),
            typeof(TransferFundsController),
            typeof(UploadMobileDataController)
        ];

        var controllers = new Dictionary<Type, Lazy<ControllerBase>>();
        foreach (var controllerType in controllerTypes)
        {
            var lazy = new Lazy<ControllerBase>(() => (ControllerBase)container.Resolve(controllerType));
            controllers.Add(controllerType, lazy);
        }

        uiContext.Initialise(controllers);
    }

    private bool IsMainThread()
    {
        return Application.Current.Dispatcher.CheckAccess();
    }

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Required here, Composition Root pattern")]
    private static void RegistrationsForReesWpf(ContainerBuilder builder)
    {
        // Wait Cursor Builder
        builder.RegisterInstance<Func<IWaitCursor>>(() => new WpfWaitCursor());

        builder.RegisterType<PersistBaxAppStateAsJson>().As<IPersistApplicationState>().SingleInstance();
        // Input Box / Message Box / Question Box / User Prompts etc
        builder.RegisterType<WpfViewLoader<InputBox>>().Named<IViewLoader>(InputBoxView);
        builder.Register(c => new WindowsInputBox(c.ResolveNamed<IViewLoader>(InputBoxView))).As<IUserInputBox>();
        builder.RegisterType<WindowsMessageBox>().As<IUserMessageBox>().SingleInstance();
        builder.RegisterType<WindowsQuestionBoxYesNo>().As<IUserQuestionBoxYesNo>().SingleInstance();
        builder.Register(_ => new Func<IUserPromptOpenFile>(() => new WindowsOpenFileDialog { AddExtension = true, CheckFileExists = true, CheckPathExists = true }))
            .SingleInstance();
        builder.Register(_ => new Func<IUserPromptSaveFile>(() => new WindowsSaveFileDialog { AddExtension = true, CheckPathExists = true }))
            .SingleInstance();
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
