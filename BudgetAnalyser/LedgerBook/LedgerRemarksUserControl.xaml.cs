using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BudgetAnalyser.LedgerBook;

/// <summary>
///     Interaction logic for LedgerRemarksUserControl.xaml
/// </summary>
public partial class LedgerRemarksUserControl : UserControl
{
    public LedgerRemarksUserControl()
    {
        InitializeComponent();
    }

    private void OnIsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
    {
        if ((bool)e.NewValue)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, () => this.RemarksTextBox.Focus());
        }
    }
}
