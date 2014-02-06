using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using BudgetAnalyser.Engine;
using Rees.Wpf;

namespace BudgetAnalyser.Statement
{
    /// <summary>
    ///     Interaction logic for StatementUserControl.xaml
    /// </summary>
    public partial class StatementUserControl : UserControl
    {
        // TODO Consider moving the edit mode to the controller, so that it controls the edit more than the view.

        private EditMode currentEditMode;
        private ListBoxItem currentRowEdit;
        private bool subscribedToMainWindowClose;

        public StatementUserControl()
        {
            MessagingGate.Register<TransactionsChangedMessage>(this, OnTransactionsChanged);
            InitializeComponent();
        }

        private enum EditMode
        {
            ReadMode,
            EditMode,
        }

        private StatementController Controller
        {
            get { return (StatementController)DataContext; }
        }

        private void ApplyFilter()
        {
            ICollectionView defaultView = CollectionViewSource.GetDefaultView(Controller.ViewModel.Statement.Transactions);
            if (string.IsNullOrWhiteSpace(Controller.ViewModel.BucketFilter))
            {
                defaultView.Filter = null;
            }
            else if (Controller.ViewModel.BucketFilter == StatementViewModel.UncategorisedFilter)
            {
                defaultView.Filter = t =>
                {
                    var txn = (Transaction)t;
                    return txn.BudgetBucket == null || string.IsNullOrWhiteSpace(txn.BudgetBucket.Code);
                };
            }
            else
            {
                defaultView.Filter = t =>
                {
                    var txn = (Transaction)t;
                    return txn.BudgetBucket != null && txn.BudgetBucket.Code == Controller.ViewModel.BucketFilter;
                };
            }
        }

        private ListBoxItem GetSelectedListBoxItem()
        {
            object transaction = this.TransactionListBox.SelectedItem;
            return (ListBoxItem)this.TransactionListBox.ItemContainerGenerator.ContainerFromItem(transaction);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!this.subscribedToMainWindowClose)
            {
                this.subscribedToMainWindowClose = true;
                Application.Current.MainWindow.Closing += OnMainWindowClosing;
            }

            if (Controller != null)
            {
                Controller.ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            }

            if (Controller == null)
            {
                ((StatementController)e.OldValue).ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
                return;
            }

            if (Controller.ViewModel.Statement == null)
            {
                return;
            }

            ApplyFilter();
        }

        /// <summary>
        ///     Edit a transaction in the list <see cref="OnTransactionSelectionChanged" /> for more details.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEditTransaction(object sender, RoutedEventArgs e)
        {
            ListBoxItem listboxItem = GetSelectedListBoxItem();
            if (listboxItem == null)
            {
                // Can happen if scroll bar is double clicked.
                return;
            }

            var currentValue = (string)listboxItem.Tag;
            if (currentValue == "True")
            {
                RestoreEditModeToRead();
                return;
            }

            listboxItem.IsSelected = true;
            this.currentEditMode = EditMode.EditMode;
            this.currentRowEdit = listboxItem;
            listboxItem.Tag = "True";
        }

        private void OnMainWindowClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            Controller.NotifyOfClosing();
        }

        private void OnTransactionListBoxDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OnEditTransaction(sender, e);
        }

        private void OnTransactionListBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                switch (this.currentEditMode)
                {
                    case EditMode.ReadMode:
                        OnEditTransaction(sender, e);
                        break;
                    case EditMode.EditMode:
                        RestoreEditModeToRead();
                        break;
                }

                e.Handled = true;
            }
        }

        private void OnTransactionListScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            this.HeaderScrollViewer.ScrollToHorizontalOffset(((ScrollViewer)sender).HorizontalOffset);
        }

        /// <summary>
        ///     This is used to swap out the item template on demand to the edit template.
        ///     The prerequisite is that the user has already executed the Edit Transaction context menu item. This event fires
        ///     after the
        ///     <see cref="OnEditTransaction" /> event.
        /// </summary>
        private void OnTransactionSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ensure the event has come from a list box. This is because the DataTemplate consists of controls (ie a ComboBox) that also fire the SelectionChanged Routed Event.
            if (!(e.OriginalSource is ListBox))
            {
                return;
            }

            // User is browsing around, there is no editing happening.
            if (this.currentEditMode == EditMode.ReadMode)
            {
                return;
            }

            // User has navigated away.
            RestoreEditModeToRead();
        }

        private void OnTransactionsChanged(TransactionsChangedMessage message)
        {
            ApplyFilter();
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BucketFilter")
            {
                ApplyFilter();
            }
        }

        private void RestoreEditModeToRead()
        {
            // User has edited the row and has navigated away from the current row.  Edit mode can be restored to ReadMode.
            this.currentRowEdit.Tag = null;
            this.currentRowEdit = null;
            this.currentEditMode = EditMode.ReadMode;
            Controller.NotifyOfEdit();

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, () => GetSelectedListBoxItem().Focus());
        }
    }
}