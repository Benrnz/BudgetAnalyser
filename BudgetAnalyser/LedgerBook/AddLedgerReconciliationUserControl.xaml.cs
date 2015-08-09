using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook
{
    /// <summary>
    ///     Interaction logic for AddLedgerReconciliationUserControl.xaml
    /// </summary>
    public partial class AddLedgerReconciliationUserControl : UserControl
    {
        public AddLedgerReconciliationUserControl()
        {
            InitializeComponent();
        }

        private void OnAddBankBalanceClick(object sender, RoutedEventArgs e)
        {
            this.BankBalance.Focus();
            this.BankBalance.SelectAll();
        }

        private void OnBankBalanceGotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;
            if (textBox != null)
            {
                textBox.SelectAll();
            }
        }

        private void OnBankBalanceMouseUp(object sender, MouseButtonEventArgs e)
        {
            this.BankBalance.SelectAll();
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool)
            {
                var visible = (bool)e.NewValue;
                if (visible)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, () => this.BankBalance.Focus());
                }
            }
        }
    }
}