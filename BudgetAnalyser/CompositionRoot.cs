using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Windows;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
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
using BudgetAnalyser.ReportsCatalog.BurnDownGraphs;
using BudgetAnalyser.ReportsCatalog.LongTermSpendingLineGraph;
using BudgetAnalyser.ReportsCatalog.OverallPerformance;
using BudgetAnalyser.Statement;
using GalaSoft.MvvmLight.Messaging;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;
using Rees.Wpf.RecentFiles;
using Rees.Wpf.UserInteraction;
using PersistApplicationStateAsXaml = BudgetAnalyser.ApplicationState.PersistApplicationStateAsXaml;

namespace BudgetAnalyser
{
    /// <summary>
    ///     This class follows the Composition Root Pattern as described here: http://blog.ploeh.dk/2011/07/28/CompositionRoot/
    ///     It constructs any and all required objects, the whole graph for use for the process lifetime of the application.
    ///     This class also contains all usage of IoC (Autofac in this case) to this class.  It should not be used any where
    ///     else in the application to prevent the Service Locator anti-pattern from appearing.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Reviewed, ok here: Necessary for composition root pattern")]
    public sealed class CompositionRoot : IDisposable
    {
        private const string InputBoxView = "InputBoxView";
        private List<IDisposable> disposables = new List<IDisposable>();
        private bool disposed;

        /// <summary>
        ///     The newly instantiated global logger ready for use.  Prior to executing the composition root the logger has not
        ///     been available.
        ///     (An alternative would be to pass the logger into the Composition Root).
        /// </summary>
        public ILogger Logger { get; private set; }

        /// <summary>
        ///     The top level Controller / ViewModel for the top level window aka <see cref="ShellWindow" />.  This is the first
        ///     object to be called following execution of this Composition Root.
        /// </summary>
        public ShellController ShellController { get; private set; }

        /// <summary>
        ///     The top level Window that binds to the <see cref="ShellController" />.
        /// </summary>
        public Window ShellWindow { get; private set; }

        /// <summary>
        ///     Register all IoC mappings and instantiate the object graph required to run the application.
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "IoC Config")]
        public void Compose()
        {
            var builder = new ContainerBuilder();
            var engineAssembly = typeof(StatementModel).GetTypeInfo().Assembly;
            var storageAssembly = typeof(IFileEncryptor).GetTypeInfo().Assembly;
            var thisAssembly = GetType().GetTypeInfo().Assembly;

            builder.RegisterAssemblyTypes(thisAssembly).AsSelf();

            ComposeTypesWithDefaultImplementations(storageAssembly, builder);
            ComposeTypesWithDefaultImplementations(engineAssembly, builder);
            ComposeTypesWithDefaultImplementations(thisAssembly, builder);

            // Register Messenger Singleton from MVVM Light
            builder.RegisterType<ConcurrentMessenger>().As<IMessenger>().SingleInstance().WithParameter("defaultMessenger", Messenger.Default);

            // Registrations from Rees.Wpf - There are no automatic registrations in this assembly.
            RegistrationsForReesWpf(builder);

            AllLocalNonAutomaticRegistrations(builder);

            BuildApplicationObjectGraph(builder, engineAssembly, thisAssembly, storageAssembly);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Check to see if Dispose has already been called. 
            if (!this.disposed)
            {
                // Release unmanaged resources. If disposing is false, 
                // only the following code is executed. 
                this.disposables.ForEach(x => x.Dispose());
                // Note that this is not thread safe. 
                // Another thread could start disposing the object 
                // after the managed resources are disposed, 
                // but before the disposed flag is set to true. 
                // If thread safety is necessary, it must be 
                // implemented by the client. 
            }

            this.disposed = true;

            // Take yourself off the Finalization queue 
            // to prevent finalization code for this object 
            // from executing a second time. 
            GC.SuppressFinalize(this);
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

                if (dependency.NamedInstanceName.IsSomething())
                {
                    // Named Dependency
                    registration = registration.Named(dependency.NamedInstanceName, dependency.DependencyRequired);
                }

                registration.AsImplementedInterfaces().AsSelf();

                // Register as custom type, other than its own class name, and directly implemented interfaces.
                if (dependency.AdditionalRegistrationType != null)
                {
                    registration.As(dependency.AdditionalRegistrationType);
                }
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "builder", Justification = "Good template code and likely use in the future")]
        private static void AllLocalNonAutomaticRegistrations(ContainerBuilder builder)
        {
            // Register any special mappings that have not been registered with automatic mappings.
            // Explicit object creation below is necessary to correctly register with IoC container.
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Required here, Composition Root pattern")]
        private static void RegistrationsForReesWpf(ContainerBuilder builder)
        {
            // Wait Cursor Builder
            builder.RegisterInstance<Func<IWaitCursor>>(() => new WpfWaitCursor());

            builder.RegisterType<AppStateRecentFileManager>().As<IRecentFileManager>().SingleInstance();
            builder.RegisterType<PersistApplicationStateAsXaml>().As<ApplicationState.IPersistApplicationState>().SingleInstance();
            // Input Box / Message Box / Question Box / User Prompts etc
            builder.RegisterType<WpfViewLoader<InputBox>>().Named<IViewLoader>(InputBoxView);
            builder.Register(c => new WindowsInputBox(c.ResolveNamed<IViewLoader>(InputBoxView))).As<IUserInputBox>();
            builder.RegisterType<WindowsMessageBox>().As<IUserMessageBox>().SingleInstance();
            builder.RegisterType<WindowsQuestionBoxYesNo>().As<IUserQuestionBoxYesNo>().SingleInstance();
            builder.Register(c => new Func<IUserPromptOpenFile>(() => new WindowsOpenFileDialog { AddExtension = true, CheckFileExists = true, CheckPathExists = true }))
                .SingleInstance();
            builder.Register(c => new Func<IUserPromptSaveFile>(() => new WindowsSaveFileDialog { AddExtension = true, CheckPathExists = true }))
                .SingleInstance();
        }

        private void BuildApplicationObjectGraph(ContainerBuilder builder, params Assembly[] assemblies)
        {
            // Instantiate and store all controllers...
            // These must be executed in the order of dependency.  For example the RulesController requires a NewRuleController so the NewRuleController must be instantiated first.
            IContainer container = builder.Build();

            Logger = container.Resolve<ILogger>();

            foreach (Assembly assembly in assemblies)
            {
                var requiredPropertyInjections = DefaultIoCRegistrations.ProcessPropertyInjection(assembly);
                foreach (PropertyInjectionDependencyRequirement requirement in requiredPropertyInjections)
                {
                    // Some reasonably awkard Autofac usage here to allow testibility.  (Extension methods aren't easy to test)
                    IComponentRegistration registration;
                    bool success = container.ComponentRegistry.TryGetRegistration(new TypedService(requirement.DependencyRequired), out registration);
                    if (success)
                    {
                        object dependency = container.ResolveComponent(registration, Enumerable.Empty<Parameter>());
                        requirement.PropertyInjectionAssignment(dependency);
                    }
                }
            }

            // Kick it off
            ConstructUiContext(container);
            this.disposables.AddIfSomething(container.Resolve<ICredentialStore>() as IDisposable);
            ShellController = container.Resolve<ShellController>();
            ShellWindow = new ShellWindow { DataContext = ShellController };
        }

        /// <summary>
        ///     Build the <see cref="IUiContext" /> instance that is used by all <see cref="ControllerBase" /> controllers.
        ///     It contains refereces to commonly used UI components that most controllers require as well as references to all
        ///     other controllers.  All controllers are single instances.
        /// </summary>
        /// <param name="container">The newly created (and short lived) IoC container used to instantiate objects.</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Composition Root Pattern - pull all abstract wiring into one place")]
        private void ConstructUiContext(IContainer container)
        {
            var uiContext = container.Resolve<UiContext>();
            uiContext.Logger = Logger;
            uiContext.ReconciliationToDoListController = container.Resolve<ReconciliationToDoListController>();
            uiContext.CreateNewFixedBudgetController = container.Resolve<CreateNewFixedBudgetController>();
            uiContext.CreateNewSurprisePaymentMonitorController = container.Resolve<CreateNewSurprisePaymentMonitorController>();
            uiContext.LedgerBucketViewController = container.Resolve<LedgerBucketViewController>();
            uiContext.ShowSurplusBalancesController = container.Resolve<ShowSurplusBalancesController>();
            uiContext.AddLedgerReconciliationController = container.Resolve<AddLedgerReconciliationController>();
            uiContext.BudgetPieController = container.Resolve<BudgetPieController>();
            uiContext.NewBudgetModelController = container.Resolve<NewBudgetModelController>();
            uiContext.BudgetController = container.Resolve<BudgetController>();
            uiContext.ChooseBudgetBucketController = container.Resolve<ChooseBudgetBucketController>();
            uiContext.LedgerRemarksController = container.Resolve<LedgerRemarksController>();
            uiContext.GlobalFilterController = container.Resolve<GlobalFilterController>();
            uiContext.LedgerTransactionsController = container.Resolve<LedgerTransactionsController>();
            uiContext.LedgerBookController = container.Resolve<LedgerBookController>();
            uiContext.CurrentMonthBurnDownGraphsController = container.Resolve<CurrentMonthBurnDownGraphsController>();
            uiContext.StatementController = container.Resolve<StatementController>();
            uiContext.NewRuleController = container.Resolve<NewRuleController>();
            uiContext.RulesController = container.Resolve<RulesController>();
            uiContext.MainMenuController = container.Resolve<MainMenuController>();
            uiContext.DashboardController = container.Resolve<DashboardController>();
            uiContext.LongTermSpendingGraphController = container.Resolve<LongTermSpendingGraphController>();
            uiContext.OverallPerformanceController = container.Resolve<OverallPerformanceController>();
            uiContext.ReportsCatalogController = container.Resolve<ReportsCatalogController>();
            uiContext.AppliedRulesController = container.Resolve<AppliedRulesController>();
            uiContext.SplitTransactionController = container.Resolve<SplitTransactionController>();
            uiContext.EditingTransactionController = container.Resolve<EditingTransactionController>();
            uiContext.StatementControllerNavigation = container.Resolve<StatementControllerNavigation>();
            uiContext.TransferFundsController = container.Resolve<TransferFundsController>();
            uiContext.DisusedRulesController = container.Resolve<DisusedRulesController>();
            uiContext.EncryptFileController = container.Resolve<EncryptFileController>();
            uiContext.UploadMobileDataController = container.Resolve<UploadMobileDataController>();
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
}