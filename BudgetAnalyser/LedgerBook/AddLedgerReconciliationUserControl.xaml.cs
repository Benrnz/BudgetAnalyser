using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BudgetAnalyser.LedgerBook;

/// <summary>
///     Interaction logic for AddLedgerReconciliationUserControl.xaml
/// </summary>
public partial class AddLedgerReconciliationUserControl : UserControl
{
    public AddLedgerReconciliationUserControl()
    {
        InitializeComponent();
    }

    private void OnAddBankBalanceClick(object? sender, RoutedEventArgs e)
    {
        this.BankBalance.Focus();
        this.BankBalance.SelectAll();
    }

    private void OnIsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is bool visible)
        {
            if (visible)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, () => this.BankBalance.Focus());
            }
        }
    }
}
