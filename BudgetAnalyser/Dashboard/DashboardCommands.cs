using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Engine.Widgets;
using GalaSoft.MvvmLight.Command;

namespace BudgetAnalyser.Dashboard
{
    public static class DashboardCommands
    {
        public static ICommand HideWidgetCommand
        {
            get { return new RelayCommand<Widget>(w => w.Visibility = false, w => w != null); }
        }

        public static ICommand UnhideAllWidgetsCommand
        {
            get { return new RelayCommand(UnhideAllWidgetsCommandExecute); }
        }

        public static IWidgetRepository WidgetRepository { get; set; }

        private static void UnhideAllWidgetsCommandExecute()
        {
            WidgetRepository.GetAll().ToList().ForEach(w => w.Visibility = true);
        }
    }
}