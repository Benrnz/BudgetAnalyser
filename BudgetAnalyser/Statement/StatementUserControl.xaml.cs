using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.ShellDialog;

namespace BudgetAnalyser.Statement
{
    /// <summary>
    ///     Interaction logic for StatementUserControl.xaml
    /// </summary>
    public partial class StatementUserControl
    {
        private bool subscribedToMainWindowClose;

        public StatementUserControl()
        {
            InitializeComponent();
        }

        private StatementController Controller
        {
            get { return (StatementController)DataContext; }
        }

        private void ApplyBucketFilter()
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
                // Once only initialisation:
                this.subscribedToMainWindowClose = true;
                Application.Current.MainWindow.Closing += OnMainWindowClosing;
                Controller.RegisterListener<TransactionsChangedMessage>(this, OnTransactionsChanged);
                Controller.RegisterListener<ShellDialogResponseMessage>(this, OnShellDialogResponseMessageReceived);
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

            ApplyBucketFilter();
        }

        private void OnMainWindowClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            Controller.NotifyOfClosing();
        }

        private void OnShellDialogResponseMessageReceived(ShellDialogResponseMessage message)
        {
            if (message.Content is EditingTransactionViewModel)
            {
                ListBoxItem listBoxItem = GetSelectedListBoxItem();
                if (listBoxItem != null)
                {
                    listBoxItem.Focus();
                }
            }
        }

        private void OnTransactionListBoxDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Controller.EditTransactionCommand.CanExecute(null))
            {
                Controller.EditTransactionCommand.Execute(null);
            }
        }

        private void OnTransactionListBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                if (Controller.EditTransactionCommand.CanExecute(null))
                {
                    Controller.EditTransactionCommand.Execute(null);
                }
            }
        }

        private void OnTransactionsChanged(TransactionsChangedMessage message)
        {
            ApplyBucketFilter();
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BucketFilter")
            {
                ApplyBucketFilter();
            }
        }
    }
}