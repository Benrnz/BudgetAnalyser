using System.Windows;

namespace BudgetAnalyser.LedgerBook;

/// <summary>
///     Interaction logic for LedgerBookView.xaml
/// </summary>
public partial class TopLedgerBookUserControl
{
    private bool subscribedToMainWindowClose;

    public TopLedgerBookUserControl()
    {
        InitializeComponent();
    }

    private TopLedgerBookController Controller => (TopLedgerBookController)DataContext;

    private void DynamicallyCreateLedgerBookGrid()
    {
        if (Controller.ViewModel.LedgerBook is null)
        {
            return;
        }

        var builder = Controller.GridBuilder();
        builder.BuildGrid(Controller.ViewModel.LedgerBook, Resources, this.LedgerBookPanel, Controller.NumberOfPeriodsToShow);
    }

    private void OnDataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
    {
        if (!this.subscribedToMainWindowClose)
        {
            this.subscribedToMainWindowClose = true;
        }

        if (e.OldValue is not null)
        {
            Controller.DeregisterListener(this);
        }

        if (e.NewValue is not null)
        {
            Controller.RegisterListener(this, (r, m) => OnLedgerBookReady(m));
            Controller.RegisterListener(this, (r, m) => OnLedgerBookUpdated(m));
        }

        DynamicallyCreateLedgerBookGrid();
    }

    private void OnLedgerBookReady(LedgerBookReadyMessage message)
    {
        // this is only used when no Ledgerbook has been previously loaded. Data binding hasnt been set up to respond to the ViewModel.LedgerBook property changing until the UI is actually drawn
        // for the first time.
        if (message.LedgerBook is not null && message.ForceUiRefresh)
        {
            DynamicallyCreateLedgerBookGrid();
        }
    }

    private void OnLedgerBookUpdated(LedgerBookUpdatedMessage message)
    {
        ResetLedgerBookContent();
        DynamicallyCreateLedgerBookGrid();
    }

    private void ResetLedgerBookContent()
    {
        this.LedgerBookPanel.Content = null;
    }
}
