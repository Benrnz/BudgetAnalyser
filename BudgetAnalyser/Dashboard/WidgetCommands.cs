using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Widgets;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.Dashboard
{
    [AutoRegisterWithIoC]
    public static class WidgetCommands
    {
        private static RelayCommand<Widget> WidgetClickedRelayCommand = new(OnWidgetCommandExecuted, WidgetCommandCanExecute);

        public static ICommand AddNewBucketMonitorWidgetCommand => new RelayCommand<Guid>(OnAddNewBucketMonitorWidgetCommandExecute);
        public static ICommand AddNewFixedBudgetMonitorWidgetCommand => new RelayCommand<Guid>(OnAddNewFixedBudgetMonitorWidgetCommandExecute);
        public static ICommand AddNewSurprisePaymentMonitorWidgetCommand => new RelayCommand<Guid>(OnAddNewSurprisePaymentMonitorWidgetCommandExecute);

        [PropertyInjection]
        public static ChooseBudgetBucketController ChooseBudgetBucketController { get; [UsedImplicitly] set; }

        [PropertyInjection]
        public static CreateNewFixedBudgetController CreateNewFixedBudgetController { get; [UsedImplicitly] set; }

        [PropertyInjection]
        public static CreateNewSurprisePaymentMonitorController CreateNewSurprisePaymentMonitorController { get; [UsedImplicitly] set; }

        [PropertyInjection]
        public static IDashboardService DashboardService { get; [UsedImplicitly] set; }

        public static ICommand HideWidgetCommand => new RelayCommand<Widget>(w => w.Visibility = false, w => w is not null);

        [PropertyInjection]
        public static IMessenger MessengerInstance { get; [UsedImplicitly] set; }

        [PropertyInjection]
        public static IUserQuestionBoxYesNo QuestionBoxService { get; [UsedImplicitly] set; }

        public static ICommand RemoveWidgetCommand => new RelayCommand<Widget>(OnRemoveWidgetCommandExecute, w => w is IUserDefinedWidget);
        public static ICommand UnhideAllWidgetsCommand => new RelayCommand(OnUnhideAllWidgetsCommandExecute);

        public static ICommand WidgetClickedCommand => WidgetClickedRelayCommand;

        public static void DeregisterForWidgetChanges(IEnumerable<WidgetGroup> widgetGroups)
        {
            if (widgetGroups is null) return;
            widgetGroups.SelectMany(w => w.Widgets)
                .ToList()
                .ForEach(w => w.PropertyChanged -= OnWidgetDataUpdated);
        }

        /// <summary>
        ///     The Dashboard must listen for widget data changes that could affect Command.CanExecute.
        /// </summary>
        public static void ListenForWidgetChanges(IEnumerable<WidgetGroup> widgetGroups)
        {
            if (widgetGroups is null) return;
            widgetGroups.SelectMany(w => w.Widgets)
                .ToList()
                .ForEach(w => w.PropertyChanged += OnWidgetDataUpdated);
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

        private static void OnRemoveWidgetCommandExecute(Widget widget)
        {
            var fixedProject = widget as FixedBudgetMonitorWidget;
            if (fixedProject is not null)
            {
                bool? result = QuestionBoxService.Show(
                    "Remove Widget",
                    "Are you sure you want to remove the Fixed Budget Project Widget '{0}'?\n\nOnce removed, it cannot be undone. All assigned transactions will be remain in the project bucket. Any unsaved edits will also be committed.",
                    fixedProject.Id);
                if (result is null || result == false)
                {
                    return;
                }
            }

            widget.PropertyChanged -= OnWidgetDataUpdated;
            DashboardService.RemoveUserDefinedWidget((IUserDefinedWidget)widget);
        }

        private static void OnUnhideAllWidgetsCommandExecute()
        {
            DashboardService.ShowAllWidgets();
        }

        private static void OnWidgetCommandExecuted(Widget widget)
        {
            MessengerInstance.Send(new WidgetActivatedMessage(widget));
        }

        private static bool WidgetCommandCanExecute(Widget widget)
        {
            return widget.Clickable;
        }

        private static void OnWidgetDataUpdated(object? sender, PropertyChangedEventArgs e)
        {
            var widget = sender as Widget;
            if (widget is null) return;
            if (e.PropertyName is nameof(widget.Clickable) or nameof(widget.Enabled))
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(WidgetClickedRelayCommand.NotifyCanExecuteChanged, DispatcherPriority.ApplicationIdle);
            }
        }
    }
}