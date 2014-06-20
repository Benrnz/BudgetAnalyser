using System.Windows;

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

        private ShellController Controller
        {
            get { return (ShellController)DataContext; }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Controller.OnViewReady();
        }
    }
}