using System.Windows;
using System.Windows.Input;

namespace BudgetAnalyser
{
    /// <summary>
    ///     Interaction logic for ShellWindow.xaml
    /// </summary>
    public partial class ShellWindow 
    {
        public ShellWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Controller.OnViewReady();
        }

        private ShellController Controller
        {
            get { return (ShellController)DataContext; }
        }

        private void OnShellDialogKeyUp(object sender, KeyEventArgs e)
        {
            if (Controller.PopUpDialogContent == null)
            {
                return;
            }

            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                Controller.DialogCommand.Execute("Ok");
            }
            else if (e.Key == Key.Escape)
            {
                Controller.DialogCommand.Execute("Cancel");
            }

            e.Handled = true;
        }
    }
}