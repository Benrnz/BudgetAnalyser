using System;
using System.Windows;
using Autofac;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.Matching;
using BudgetAnalyser.ReportsCatalog;
using BudgetAnalyser.BurnDownGraphs;
using BudgetAnalyser.Statement;

using GalaSoft.MvvmLight.Messaging;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;
using Rees.Wpf.ApplicationState;
using Rees.Wpf.RecentFiles;
using Rees.Wpf.UserInteraction;

namespace BudgetAnalyser
{
    public class CompositionRoot
    {
        private const string InputBoxView = "InputBoxView";

        public ShellController ShellController { get; private set; }
        public Window ShellWindow { get; private set; }
        public ILogger Logger { get; private set; }

        public void RegisterIoCMappings()
        {
            var builder = new ContainerBuilder();

            DefaultIoCRegistrations.RegisterDefaultMappings(builder);
            IoC.RegisterAutoMappingsFromAssembly(builder, GetType().Assembly);


            // Wait Cursor Builder
            builder.RegisterInstance<Func<IWaitCursor>>(() => new WpfWaitCursor());

            // Registrations from Rees.Wpf
            builder.RegisterType<XmlRecentFileManager>().As<IRecentFileManager>().SingleInstance();
            builder.RegisterType<PersistApplicationStateAsXaml>().As<IPersistApplicationState>().SingleInstance();
            // Input Box / Message Box / Question Box / User Prompts etc
            builder.RegisterType<WpfViewLoader<InputBox>>().Named<IViewLoader>(InputBoxView);
            builder.Register(c => new WindowsInputBox(c.ResolveNamed<IViewLoader>(InputBoxView))).As<IUserInputBox>();
            builder.RegisterType<WindowsMessageBox>().As<IUserMessageBox>().SingleInstance();
            builder.RegisterType<WindowsQuestionBoxYesNo>().As<IUserQuestionBoxYesNo>().SingleInstance();
            builder.Register(c => new Func<IUserPromptOpenFile>(() => new WindowsOpenFileDialog { AddExtension = true, CheckFileExists = true, CheckPathExists = true }))
                .SingleInstance();
            builder.Register(c => new Func<IUserPromptSaveFile>(() => new WindowsSaveFileDialog { AddExtension = true, CheckPathExists = true }))
                .SingleInstance();


            builder.RegisterInstance<Func<BucketBurnDownController>>(() => new BucketBurnDownController(new BurnDownGraphAnalyser()));


            // Register Messenger Singleton from MVVM Light
            builder.RegisterType<ConcurrentMessenger>().As<IMessenger>().SingleInstance().WithParameter("defaultMessenger", Messenger.Default);
            
            // Explicit object creation below is necessary to correctly register with IoC container.
            // ReSharper disable once RedundantDelegateCreation
            builder.Register(c => new Func<object, IPersistent>(model => new RecentFilesPersistentModelV1(model))).SingleInstance();
            

            // Instantiate and store all controllers...
            // These must be executed in the order of dependency.  For example the RulesController requires a NewRuleController so the NewRuleController must be instantiated first.
            var container = builder.Build();

            Logger = container.Resolve<ILogger>();

            // Kick it off
            ConstructUiContext(container);
            ShellController = container.Resolve<ShellController>();
            ShellWindow = new ShellWindow { DataContext = ShellController };
        }

        private void ConstructUiContext(IContainer container)
        {
            var uiContext = container.Resolve<UiContext>();
            uiContext.Logger = Logger;
            uiContext.AddLedgerReconciliationController = container.Resolve<AddLedgerReconciliationController>();
            uiContext.BudgetPieController = container.Resolve<BudgetPieController>();
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
            uiContext.ReportsCatalogController = container.Resolve<ReportsCatalogController>();
            uiContext.AppliedRulesController = container.Resolve<AppliedRulesController>();
        }
    }
}