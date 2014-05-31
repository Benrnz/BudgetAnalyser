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
        private bool subscribedToControllerPropertyChanged;
        private bool subscribedToMainWindowClose;

        public LedgerBookUserControl()
        {
            InitializeComponent();
        }

        private LedgerBookController Controller
        {
            get { return DataContext as LedgerBookController; }
        }
        
        private void OnControllerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LedgerBook")
            {
                DynamicallyCreateLedgerBookGrid();
            }
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

            if (this.subscribedToControllerPropertyChanged && e.OldValue != null)
            {
                Controller.PropertyChanged -= OnControllerPropertyChanged;
            }
            else if (!this.subscribedToControllerPropertyChanged && e.NewValue != null)
            {
                this.subscribedToControllerPropertyChanged = true;
                Controller.PropertyChanged += OnControllerPropertyChanged;
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