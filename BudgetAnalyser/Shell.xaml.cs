using System.Windows;
using System.Windows.Input;
using BudgetAnalyser.ShellDialog;

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
    }
}