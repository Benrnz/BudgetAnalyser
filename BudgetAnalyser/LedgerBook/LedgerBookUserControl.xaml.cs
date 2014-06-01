using System;
using System.ComponentModel;
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
            var builder = Controller.GridBuilder();
            builder.BuildGrid(Controller.ViewModel.LedgerBook, Resources, LedgerBookPanel);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!this.subscribedToMainWindowClose)
            {
                this.subscribedToMainWindowClose = true;
                Application.Current.MainWindow.Closing += OnMainWindowClosing;
            }

            if (e.OldValue != null)
            {
                ((LedgerBookController)e.OldValue).LedgerBookUpdated -= OnLedgerBookUpdated;
            }

            if (e.NewValue != null)
            {
                ((LedgerBookController)e.NewValue).LedgerBookUpdated += OnLedgerBookUpdated;
            }

            DynamicallyCreateLedgerBookGrid();
        }

        private void OnLedgerBookNameClick(object sender, MouseButtonEventArgs e)
        {
            Controller.EditLedgerBookName();
        }

        private void OnLedgerBookUpdated(object sender, EventArgs e)
        {
            ResetLedgerBookContent();
            DynamicallyCreateLedgerBookGrid();
        }

        private void OnMainWindowClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            Controller.NotifyOfClosing();
        }

        private void ResetLedgerBookContent()
        {
            this.LedgerBookPanel.Content = null;
        }
    }
}