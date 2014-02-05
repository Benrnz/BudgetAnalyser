using System;
using System.Windows;
using Autofac;
using Autofac.Core;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;
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
        private const string SpendingTrendView = "SpendingTrendView";
        private const string UserDefinedChartDialog = "UserDefinedChartDialog";
        private const string DateFilterView = "DateFilterView";
        private const string AccountTypeFilterView = "AccountTypeFilterView";
        private const string LoadFileView = "LoadFileView";
        private const string MaintainRulesView = "MaintainRulesView";
        private const string NewRuleView = "NewRuleView";
        private const string InputBoxView = "InputBoxView";

        public ShellController ShellController { get; private set; }
        public Window ShellWindow { get; private set; }

        public void RegisterIoCMappings()
        {
            var builder = new ContainerBuilder();

            DefaultIoCRegistrations.RegisterDefaultMappings(builder);

            IoC.RegisterAutoMappingsFromAssembly(builder, GetType().Assembly);


            // Everything else:
            builder.RegisterInstance<Func<IWaitCursor>>(() => new WpfWaitCursor());

            // Input Box / Message Box / Question Box / User Prompts etc
            builder.RegisterType<WpfViewLoader<InputBox>>().Named<IViewLoader>(InputBoxView);
            builder.Register(c => new WindowsInputBox(c.ResolveNamed<IViewLoader>(InputBoxView))).As<IUserInputBox>();
            builder.RegisterType<WindowsMessageBox>().As<IUserMessageBox>().SingleInstance();
            builder.RegisterType<WindowsQuestionBoxYesNo>().As<IUserQuestionBoxYesNo>().SingleInstance();
            builder.Register(c => new Func<IUserPromptOpenFile>(() => new WindowsOpenFileDialog { AddExtension = true, CheckFileExists = true, CheckPathExists = true }))
                .SingleInstance();
            builder.Register(c => new Func<IUserPromptSaveFile>(() => new WindowsSaveFileDialog { AddExtension = true, CheckPathExists = true }))
                .SingleInstance();


            // Load File View and Controller
            builder.RegisterType<WpfViewLoader<LoadFileView>>().Named<IViewLoader>(LoadFileView).SingleInstance();
            builder.Register(c => new LoadFileController(
                c.ResolveNamed<IViewLoader>(LoadFileView),
                c.Resolve<UiContext>(),
                c.Resolve<IAccountTypeRepository>(),
                c.Resolve<IStatementModelRepository>()))
                .SingleInstance();

            

            // Rules
            builder.RegisterType<CreateNewRuleViewLoader>().Named<IViewLoader>(NewRuleView).SingleInstance();
            builder.RegisterType<NewRuleController>().SingleInstance().WithParameter(ResolvedParameter.ForNamed<IViewLoader>(NewRuleView));
            builder.RegisterType<WpfViewLoader<MaintainRulesView>>().Named<IViewLoader>(MaintainRulesView).SingleInstance();
            builder.RegisterType<RulesController>().SingleInstance().WithParameter(ResolvedParameter.ForNamed<IViewLoader>(MaintainRulesView));



            // Statement Controller
            builder.RegisterInstance<Func<BucketSpendingController>>(() => new BucketSpendingController(new SpendingGraphAnalyser()));
            builder.RegisterType<WpfViewLoader<SpendingTrendView>>().Named<IViewLoader>(SpendingTrendView).SingleInstance();
            builder.RegisterType<WpfViewLoader<AddUserDefinedSpendingChartDialog>>().Named<IViewLoader>(UserDefinedChartDialog).SingleInstance();
            builder.RegisterType<AddUserDefinedSpendingChartController>().SingleInstance().WithParameter(ResolvedParameter.ForNamed<IViewLoader>(UserDefinedChartDialog));
            builder.RegisterType<SpendingTrendController>().SingleInstance().WithParameter(ResolvedParameter.ForNamed<IViewLoader>(SpendingTrendView));


            // Budget Controller
            var budgetDetailsViewLoader = new WpfViewLoader<BudgetDetailsView>();
            var budgetSelectionViewLoader = new WpfViewLoader<BudgetSelectionView>();
            builder.Register(c => new BudgetController(
                                      c.Resolve<IBudgetRepository>(),
                                      c.Resolve<UiContext>(), 
                                      budgetDetailsViewLoader, 
                                      budgetSelectionViewLoader,
                                      c.Resolve<DemoFileHelper>()));


            // Filters
            builder.RegisterType<WpfViewLoader<GlobalDateFilterView>>().Named<IViewLoader>(DateFilterView).SingleInstance();
            builder.RegisterType<WpfViewLoader<GlobalAccountTypeFilterView>>().Named<IViewLoader>(AccountTypeFilterView).SingleInstance();
            builder.Register(c => new GlobalFilterController(
                c.Resolve<IUserMessageBox>(), 
                c.ResolveNamed<IViewLoader>(DateFilterView), 
                c.ResolveNamed<IViewLoader>(AccountTypeFilterView)));


            // Shell Controller and Application State
            builder.RegisterInstance(Messenger.Default).As<IMessenger>();
            builder.Register(c => new Func<object, IPersistent>(model => new RecentFilesPersistentModelV1(model))).SingleInstance();
            builder.RegisterType<XmlRecentFileManager>().As<IRecentFileManager>().SingleInstance();

            builder.RegisterType<PersistApplicationStateAsXaml>().As<IPersistApplicationState>().SingleInstance();



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

            
            // Kick it off
            ShellWindow = new ShellWindow { DataContext = ShellController };
            ShellController = container.Resolve<ShellController>();
        }

    }
}