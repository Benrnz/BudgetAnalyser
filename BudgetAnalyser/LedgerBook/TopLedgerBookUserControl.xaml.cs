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
            ((TopLedgerBookController)e.OldValue).LedgerBookUpdated -= OnLedgerBookUpdated;
            ((TopLedgerBookController)e.OldValue).DeregisterListener(this);
        }

        if (e.NewValue is not null)
        {
            ((TopLedgerBookController)e.NewValue).LedgerBookUpdated += OnLedgerBookUpdated;
            Controller.RegisterListener(this,  (r, m) => OnLedgerBookReadyMessageReceived(m));
        }

        DynamicallyCreateLedgerBookGrid();
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

    private void ResetLedgerBookContent()
    {
        this.LedgerBookPanel.Content = null;
    }
}
