using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook
{
    /// <summary>
    ///     Interaction logic for LedgerTransactionsUserControl.xaml
    /// </summary>
    public partial class LedgerTransactionsUserControl : UserControl
    {
        public LedgerTransactionsUserControl()
        {
            InitializeComponent();
        }

        private LedgerTransactionsController Controller
        {
            get { return DataContext as LedgerTransactionsController; }
        }

        private void OnNewTransactionVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool) e.NewValue)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, () => this.NewTransactionNarrativeTextBox.Focus());
            }
        }
    }
}