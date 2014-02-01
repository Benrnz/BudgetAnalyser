using System.Windows;

namespace BudgetAnalyser
{
    /// <summary>
    ///     Interaction logic for ShellWindow.xaml
    /// </summary>
    public partial class ShellWindow : Window
    {
        public ShellWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ((ShellController)DataContext).OnViewReady();
        }
    }
}