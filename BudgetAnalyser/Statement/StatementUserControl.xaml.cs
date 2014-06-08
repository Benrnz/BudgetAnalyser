using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.ShellDialog;

namespace BudgetAnalyser.Statement
{
    /// <summary>
    ///     Interaction logic for StatementUserControl.xaml
    /// </summary>
    public partial class StatementUserControl
    {
        private const double BucketComboMaxWidth = 130;
        private const double BucketComboMinWidth = 37;
        private const double ClearSearchButtonMaxWidth = 40;
        private const double SearchBoxMaxWidth = 130;
        private const double SearchBoxMinWidth = 27;
        private bool subscribedToMainWindowClose;

        public StatementUserControl()
        {
            InitializeComponent();
        }

        private StatementController Controller
        {
            get { return (StatementController)DataContext; }
        }

        private static void AnimateWidth(FrameworkElement element, double from, double to)
        {
            var storyboard = new Storyboard();
            var fade = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromSeconds(1),
            };

            Storyboard.SetTarget(fade, element);
            Storyboard.SetTargetProperty(fade, new PropertyPath(WidthProperty));
            storyboard.Children.Add(fade);
            storyboard.Begin();
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

        private void OnBucketFilterComboBoxDropDownClosed(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Controller.ViewModel.BucketFilter))
            {
                return;
            }

            AnimateWidth(this.BucketFilterComboBox, BucketComboMaxWidth, BucketComboMinWidth);
        }

        private void OnBucketFilterComboBoxDropDownOpened(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Controller.ViewModel.BucketFilter))
            {
                return;
            }

            AnimateWidth(this.BucketFilterComboBox, BucketComboMinWidth, BucketComboMaxWidth);
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
                Controller.RegisterListener<NavigateToTransactionMessage>(this, OnNavigateToTransactionRequestReceived);
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
            Controller.FileOperations.NotifyOfClosing();
        }

        private void OnNavigateToTransactionRequestReceived(NavigateToTransactionMessage message)
        {
            message.WhenReadyToNavigate.ContinueWith(t =>
            {
                if (t.IsCompleted && !t.IsCanceled && !t.IsFaulted && message.Success)
                {
                    IsVisibleChanged += OnVisibleChangedShowTransaction;
                }
            });
        }

        private void OnSearchTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            AnimateWidth(this.SearchTextBox, SearchBoxMinWidth, SearchBoxMaxWidth);
            AnimateWidth(this.ClearSearchButton, 0, ClearSearchButtonMaxWidth);
        }

        private void OnSearchTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            AnimateWidth(this.SearchTextBox, SearchBoxMaxWidth, SearchBoxMinWidth);
            AnimateWidth(this.ClearSearchButton, ClearSearchButtonMaxWidth, 0);
        }

        private void OnShellDialogResponseMessageReceived(ShellDialogResponseMessage message)
        {
            if (message.Content is EditingTransactionController)
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

        private void OnVisibleChangedShowTransaction(object sender, DependencyPropertyChangedEventArgs e)
        {
            IsVisibleChanged -= OnVisibleChangedShowTransaction;

            this.TransactionListBox.UpdateLayout();
            this.TransactionListBox.ScrollIntoView(Controller.ViewModel.SelectedRow);
        }
    }
}