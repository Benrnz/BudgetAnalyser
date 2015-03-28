using System;
using System.Windows.Input;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Widgets;
using GalaSoft.MvvmLight.CommandWpf;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.Dashboard
{
    [AutoRegisterWithIoC]
    public static class DashboardCommands
    {
        public static ICommand AddNewBucketMonitorWidgetCommand
        {
            get { return new RelayCommand<Guid>(OnAddNewBucketMonitorWidgetCommandExecute); }
        }

        public static ICommand AddNewFixedBudgetMonitorWidgetCommand
        {
            get { return new GalaSoft.MvvmLight.Command.RelayCommand<Guid>(OnAddNewFixedBudgetMonitorWidgetCommandExecute); }
        }

        public static ICommand AddNewSurprisePaymentMonitorWidgetCommand
        {
            get { return new GalaSoft.MvvmLight.Command.RelayCommand<Guid>(OnAddNewSurprisePaymentMonitorWidgetCommandExecute); }
        }

        [PropertyInjection]
        public static ChooseBudgetBucketController ChooseBudgetBucketController { get; [UsedImplicitly] set; }

        [PropertyInjection]
        public static CreateNewFixedBudgetController CreateNewFixedBudgetController { get; [UsedImplicitly] set; }

        [PropertyInjection]
        public static CreateNewSurprisePaymentMonitorController CreateNewSurprisePaymentMonitorController { get; [UsedImplicitly] set; }

        [PropertyInjection]
        public static IDashboardService DashboardService { get; [UsedImplicitly] set; }

        [PropertyInjection]
        public static IUserQuestionBoxYesNo QuestionBoxService { get; [UsedImplicitly] set; }

        public static ICommand HideWidgetCommand
        {
            get { return new RelayCommand<Widget>(w => w.Visibility = false, w => w != null); }
        }

        public static ICommand RemoveWidgetCommand
        {
            get
            {
                return new RelayCommand<Widget>(OnRemoveWidgetCommandExecute, w => w is IUserDefinedWidget);
            }
        }

        private static void OnRemoveWidgetCommandExecute(Widget widget)
        {
            var fixedProject = widget as FixedBudgetMonitorWidget;
            if (fixedProject != null)
            {
                var result = QuestionBoxService.Show(
                    "Remove Widget",
                    "Are you sure you want to remove the Fixed Budget Project Widget '{0}'?\n\nOnce removed, it cannot be undone. All assigned transactions will be remain in the project bucket. Any unsaved edits will also be committed.",
                    fixedProject.Id);
                if (result == null || result == false) return;
            }

            DashboardService.RemoveUserDefinedWidget((IUserDefinedWidget)widget);
        }

        public static ICommand UnhideAllWidgetsCommand
        {
            get { return new RelayCommand(OnUnhideAllWidgetsCommandExecute); }
        }

        private static void OnAddNewBucketMonitorWidgetCommandExecute(Guid correlationId)
        {
            ChooseBudgetBucketController.Filter(b => b is ExpenseBucket, "Expense Buckets");
            ChooseBudgetBucketController.ShowDialog(BudgetAnalyserFeature.Dashboard, "New Budget Bucket Monitor Widget", correlationId);
        }

        private static void OnAddNewFixedBudgetMonitorWidgetCommandExecute(Guid correlationId)
        {
            CreateNewFixedBudgetController.ShowDialog(BudgetAnalyserFeature.Dashboard, correlationId);
        }

        private static void OnAddNewSurprisePaymentMonitorWidgetCommandExecute(Guid correlationId)
        {
            CreateNewSurprisePaymentMonitorController.ShowDialog(BudgetAnalyserFeature.Dashboard, correlationId);
        }

        private static void OnUnhideAllWidgetsCommandExecute()
        {
            DashboardService.ShowAllWidgets();
        }
    }
}