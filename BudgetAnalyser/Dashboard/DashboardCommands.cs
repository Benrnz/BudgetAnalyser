using System;
using System.Windows.Input;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Widgets;
using GalaSoft.MvvmLight.CommandWpf;

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

        [PropertyInjection]
        public static ChooseBudgetBucketController ChooseBudgetBucketController { get; set; }

        [PropertyInjection]
        public static CreateNewFixedBudgetController CreateNewFixedBudgetController { get; set; }

        [PropertyInjection]
        public static IDashboardService DashboardService { get; set; }

        public static ICommand HideWidgetCommand
        {
            get { return new RelayCommand<Widget>(w => w.Visibility = false, w => w != null); }
        }

        public static ICommand RemoveWidgetCommand
        {
            get { return new RelayCommand<Widget>(w => DashboardService.RemoveUserDefinedWidget((IUserDefinedWidget)w), w => w is BudgetBucketMonitorWidget); }
        }

        public static ICommand UnhideAllWidgetsCommand
        {
            get { return new RelayCommand(UnhideAllWidgetsCommandExecute); }
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

        private static void UnhideAllWidgetsCommandExecute()
        {
            DashboardService.ShowAllWidgets();
        }
    }
}