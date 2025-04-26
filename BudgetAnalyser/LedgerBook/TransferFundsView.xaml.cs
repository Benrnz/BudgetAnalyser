using System.Windows;

namespace BudgetAnalyser.LedgerBook;

/// <summary>
///     Interaction logic for TransferFundsView.xaml
/// </summary>
public partial class TransferFundsView
{
    public TransferFundsView()
    {
        InitializeComponent();
    }

    private TransferFundsController? Controller => (TransferFundsController)DataContext;

    private void Amount_OnSourceUpdated(object? sender, RoutedEventArgs e)
    {
        Controller?.RequerySuggested();
    }

    private void FromLedger_OnSelected(object? sender, RoutedEventArgs e)
    {
        Controller?.RequerySuggested();
    }

    private void Narrative_OnSourceUpdated(object? sender, RoutedEventArgs e)
    {
        Controller?.RequerySuggested();
    }

    private void ToLedger_OnSelected(object? sender, RoutedEventArgs e)
    {
        Controller?.RequerySuggested();
    }
}
