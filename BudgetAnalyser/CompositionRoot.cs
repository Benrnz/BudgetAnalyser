using System;
using System.Windows;
using Autofac;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.ReportsCatalog;
using BudgetAnalyser.SpendingTrend;
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

        public void RegisterIoCMappings()
        {
            var builder = new ContainerBuilder();

            Engine.DefaultIoCRegistrations.RegisterDefaultMappings(builder);
            Engine.IoC.RegisterAutoMappingsFromAssembly(builder, GetType().Assembly);


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


            builder.RegisterInstance<Func<BucketSpendingController>>(() => new BucketSpendingController(new SpendingGraphAnalyser()));


            // Register Messenger Singleton from MVVM Light
            builder.RegisterInstance(Messenger.Default).As<IMessenger>();
            
            // Explicit object creation below is necessary to correctly register with IoC container.
            // ReSharper disable once RedundantDelegateCreation
            builder.Register(c => new Func<object, IPersistent>(model => new RecentFilesPersistentModelV1(model))).SingleInstance();
            

            // Instantiate and store all controllers...
            var container = builder.Build();
            var uiContext = container.Resolve<UiContext>();
            uiContext.AddLedgerReconciliationController = container.Resolve<AddLedgerReconciliationController>();
            uiContext.BudgetPieController = container.Resolve<BudgetPieController>();
            uiContext.BudgetController = container.Resolve<BudgetController>();
            uiContext.ChooseBudgetBucketController = container.Resolve<ChooseBudgetBucketController>();
            uiContext.LedgerRemarksController = container.Resolve<LedgerRemarksController>();
            uiContext.GlobalFilterController = container.Resolve<GlobalFilterController>();
            uiContext.LedgerTransactionsController = container.Resolve<LedgerTransactionsController>();
            uiContext.LedgerBookController = container.Resolve<LedgerBookController>();
            uiContext.SpendingTrendController = container.Resolve<SpendingTrendController>();
            uiContext.StatementController = container.Resolve<StatementController>();
            uiContext.RulesController = container.Resolve<RulesController>();
            uiContext.MainMenuController = container.Resolve<MainMenuController>();
            uiContext.DashboardController = container.Resolve<DashboardController>();
            uiContext.ReportsCatalogController = container.Resolve<ReportsCatalogController>();
            uiContext.AppliedRulesController = container.Resolve<AppliedRulesController>();

            
            // Kick it off
            ShellController = container.Resolve<ShellController>();
            ShellWindow = new ShellWindow { DataContext = ShellController };
        }

    }
}