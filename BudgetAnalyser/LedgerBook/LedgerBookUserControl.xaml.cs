using System.Windows;
using System.Windows.Input;

namespace BudgetAnalyser.LedgerBook;

/// <summary>
///     Interaction logic for LedgerBookView.xaml
/// </summary>
public partial class LedgerBookUserControl
{
    private bool subscribedToMainWindowClose;

    public LedgerBookUserControl()
    {
        InitializeComponent();
    }

    private LedgerBookController Controller => (LedgerBookController)DataContext;

    private void DynamicallyCreateLedgerBookGrid()
    {
        if (Controller.ViewModel.LedgerBook is null)
        {
            return;
        }

        var builder = Controller.GridBuilder();
        builder.BuildGrid(Controller.ViewModel.LedgerBook, Resources, this.LedgerBookPanel, Controller.NumberOfPeriodsToShow);
    }

    private void OnAddLedgerClicked(object? sender, RoutedEventArgs e)
    {
        Controller.OnAddNewLedgerCommandExecuted();
    }

    private void OnAddNewReconciliationClicked(object? sender, RoutedEventArgs e)
    {
        Controller.OnAddNewReconciliationCommandExecuted();
    }

    private void OnDataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
    {
        if (!this.subscribedToMainWindowClose)
        {
            this.subscribedToMainWindowClose = true;
        }

        if (e.OldValue is not null)
        {
            ((LedgerBookController)e.OldValue).LedgerBookUpdated -= OnLedgerBookUpdated;
            ((LedgerBookController)e.OldValue).DeregisterListener(this);
        }

        if (e.NewValue is not null)
        {
            ((LedgerBookController)e.NewValue).LedgerBookUpdated += OnLedgerBookUpdated;
            Controller.RegisterListener(this, static (r, m) => r.OnLedgerBookReadyMessageReceived(m));
        }

        DynamicallyCreateLedgerBookGrid();
    }

    private void OnLedgerBookNameClick(object? sender, MouseButtonEventArgs e)
    {
        Controller.EditLedgerBookName();
    }

    private void OnLedgerBookReadyMessageReceived(LedgerBookReadyMessage message)
    {
        // this is only used when no Ledgerbook has been previously loaded. Data binding hasnt been set up to respond to the ViewModel.LedgerBook property changing until the UI is actually drawn
        // for the first time.
        if (message.LedgerBook is not null && message.ForceUiRefresh)
        {
            DynamicallyCreateLedgerBookGrid();
        }
    }

    private void OnLedgerBookUpdated(object? sender, EventArgs e)
    {
        ResetLedgerBookContent();
        DynamicallyCreateLedgerBookGrid();
    }

    private void OnTransferFundsClicked(object? sender, RoutedEventArgs e)
    {
        Controller.OnTransferFundsInitiated();
    }

    private void OnUnlockCurrentLedgerLineClicked(object? sender, RoutedEventArgs e)
    {
        Controller.OnUnlockLedgerLineCommandExecuted();
    }

    private void ResetLedgerBookContent()
    {
        this.LedgerBookPanel.Content = null;
    }
}
