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

        [PropertyInjection]
        public static ChooseBudgetBucketController ChooseBudgetBucketController { get; set; }

        public static ICommand HideWidgetCommand
        {
            get { return new RelayCommand<Widget>(w => w.Visibility = false, w => w != null); }
        }

        public static ICommand RemoveWidgetCommand
        {
            get { return new RelayCommand<Widget>(w => DashboardService.RemoveMultiInstanceWidget((IMultiInstanceWidget)w), w => w is BudgetBucketMonitorWidget); }
        }

        public static ICommand UnhideAllWidgetsCommand
        {
            get { return new RelayCommand(UnhideAllWidgetsCommandExecute); }
        }

        [PropertyInjection]
        public static IDashboardService DashboardService { get; set; }

        private static void OnAddNewBucketMonitorWidgetCommandExecute(Guid correlationId)
        {
            ChooseBudgetBucketController.Filter(b => b is ExpenseBucket, "Expense Buckets");
            ChooseBudgetBucketController.ShowDialog(BudgetAnalyserFeature.Dashboard, "New Budget Bucket Monitor Widget", correlationId);
        }

        private static void UnhideAllWidgetsCommandExecute()
        {
            DashboardService.ShowAllWidgets();
        }
    }
}