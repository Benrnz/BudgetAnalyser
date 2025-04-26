using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BudgetAnalyser.LedgerBook;

/// <summary>
///     Interaction logic for LedgerTransactionsUserControl.xaml
/// </summary>
public partial class LedgerTransactionsUserControl : UserControl
{
    public LedgerTransactionsUserControl()
    {
        InitializeComponent();
    }

    private void OnNewTransactionVisibilityChanged(object? sender, DependencyPropertyChangedEventArgs e)
    {
        if ((bool)e.NewValue)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, () => this.NewTransactionNarrativeTextBox.Focus());
        }
    }
}
