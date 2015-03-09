using System;
using System.Windows;
using System.Windows.Input;

namespace BudgetAnalyser.LedgerBook
{
    /// <summary>
    ///     Interaction logic for LegderBookView.xaml
    /// </summary>
    public partial class LedgerBookUserControl
    {
        private bool subscribedToMainWindowClose;

        public LedgerBookUserControl()
        {
            InitializeComponent();
        }

        private LedgerBookController Controller
        {
            get { return DataContext as LedgerBookController; }
        }

        private void DynamicallyCreateLedgerBookGrid()
        {
            ILedgerBookGridBuilder builder = Controller.GridBuilder();
            builder.BuildGrid(Controller.ViewModel.LedgerBook, Resources, this.LedgerBookPanel, Controller.NumberOfMonthsToShow);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!this.subscribedToMainWindowClose)
            {
                this.subscribedToMainWindowClose = true;
            }

            if (e.OldValue != null)
            {
                ((LedgerBookController)e.OldValue).LedgerBookUpdated -= OnLedgerBookUpdated;
                ((LedgerBookController)e.OldValue).DeregisterListener<LedgerBookReadyMessage>(this, OnLedgerBookReadyMessageReceived);
            }

            if (e.NewValue != null)
            {
                ((LedgerBookController)e.NewValue).LedgerBookUpdated += OnLedgerBookUpdated;
                Controller.RegisterListener<LedgerBookReadyMessage>(this, OnLedgerBookReadyMessageReceived);
            }

            DynamicallyCreateLedgerBookGrid();
        }

        private void OnLedgerBookNameClick(object sender, MouseButtonEventArgs e)
        {
            Controller.EditLedgerBookName();
        }

        private void OnLedgerBookReadyMessageReceived(LedgerBookReadyMessage message)
        {
            // this is only used when no Ledgerbook has been previously loaded. Data binding hasnt been set up to respond to the ViewModel.LedgerBook property changing until the UI is actually drawn 
            // for the first time.
            if (message.LedgerBook != null && message.ForceUiRefresh)
            {
                DynamicallyCreateLedgerBookGrid();
            }
        }

        private void OnLedgerBookUpdated(object sender, EventArgs e)
        {
            ResetLedgerBookContent();
            DynamicallyCreateLedgerBookGrid();
        }

        private void ResetLedgerBookContent()
        {
            this.LedgerBookPanel.Content = null;
        }
    }
}