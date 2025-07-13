using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.ShellDialog;

namespace BudgetAnalyser.Statement;

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

    private TabTransactionsController Controller => (TabTransactionsController)DataContext;

    private static void AnimateWidth(FrameworkElement element, double from, double to)
    {
        var storyboard = new Storyboard();
        var fade = new DoubleAnimation
        {
            From = from,
            To = to,
            Duration = TimeSpan.FromSeconds(1)
        };

        Storyboard.SetTarget(fade, element);
        Storyboard.SetTargetProperty(fade, new PropertyPath(WidthProperty));
        storyboard.Children.Add(fade);
        storyboard.Begin();
    }

    private void ApplyBucketFilter()
    {
        var defaultView = CollectionViewSource.GetDefaultView(Controller.ViewModel.Transactions);
        if (string.IsNullOrWhiteSpace(Controller.BucketFilter))
        {
            defaultView.Filter = null;
        }
        else
        {
            defaultView.Filter = Controller.BucketFilter == TransactionConstants.UncategorisedFilter
                ? t =>
                {
                    var txn = (Transaction)t;
                    return txn.BudgetBucket is null || string.IsNullOrWhiteSpace(txn.BudgetBucket.Code);
                }
            : t =>
            {
                var txn = (Transaction)t;
                return txn.BudgetBucket is not null && txn.BudgetBucket.Code == Controller.BucketFilter;
            };
        }
    }

    private void ClearSearchButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Controller.ClearSearch();
        this.ClearSearchButton.IsEnabled = false;
    }

    private ListBoxItem GetSelectedListBoxItem()
    {
        var transaction = this.TransactionListBox.SelectedItem;
        return (ListBoxItem)this.TransactionListBox.ItemContainerGenerator.ContainerFromItem(transaction);
    }

    private void OnBucketFilterComboBoxDropDownClosed(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(Controller.BucketFilter))
        {
            return;
        }

        AnimateWidth(this.BucketFilterComboBox, BucketComboMaxWidth, BucketComboMinWidth);
    }

    private void OnBucketFilterComboBoxDropDownOpened(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(Controller.BucketFilter))
        {
            return;
        }

        AnimateWidth(this.BucketFilterComboBox, BucketComboMinWidth, BucketComboMaxWidth);
    }

    private void OnDataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
    {
        if (!this.subscribedToMainWindowClose)
        {
            // Once only initialisation:
            this.subscribedToMainWindowClose = true;
            Controller.RegisterListener<TransactionsChangedMessage>(this, static (r, m) => r.OnTransactionsChanged(m));
            Controller.RegisterListener<ShellDialogResponseMessage>(this, static (r, m) => r.OnShellDialogResponseMessageReceived(m));
        }

        if (Controller is not null)
        {
            Controller.ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        if (Controller is null)
        {
            ((TabTransactionsController)e.OldValue).ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            return;
        }

        if (Controller.ViewModel.Statement is null)
        {
            return;
        }

        ApplyBucketFilter();
    }

    private void OnNavigateNextClick(object sender, RoutedEventArgs e)
    {
        Controller.NavigateNextPage();
    }

    private void OnNavigatePreviousClick(object sender, RoutedEventArgs e)
    {
        Controller.NavigatePreviousPage();
    }

    private void OnSearchTextBoxGotFocus(object? sender, RoutedEventArgs e)
    {
        AnimateWidth(this.SearchTextBox, SearchBoxMinWidth, SearchBoxMaxWidth);
        AnimateWidth(this.ClearSearchButton, 0, ClearSearchButtonMaxWidth);
    }

    private void OnSearchTextBoxLostFocus(object? sender, RoutedEventArgs e)
    {
        AnimateWidth(this.SearchTextBox, SearchBoxMaxWidth, SearchBoxMinWidth);
        AnimateWidth(this.ClearSearchButton, ClearSearchButtonMaxWidth, 0);
    }

    private void OnShellDialogResponseMessageReceived(ShellDialogResponseMessage message)
    {
        if (message.Content is EditingTransactionController)
        {
            var listBoxItem = GetSelectedListBoxItem();
            listBoxItem?.Focus();
        }
    }

    private void OnTransactionListBoxDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        if (Controller.EditTransactionCommand.CanExecute(null))
        {
            Controller.EditTransactionCommand.Execute(null);
        }
    }

    private void OnTransactionListBoxKeyUp(object? sender, KeyEventArgs e)
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

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "BucketFilter")
        {
            if (Math.Abs(this.SearchTextBox.Width - BucketComboMaxWidth) > 0.01)
            {
                OnBucketFilterComboBoxDropDownOpened(sender, e);
            }

            ApplyBucketFilter();
        }
    }

    private void OnVisibleChangedShowTransaction(object? sender, DependencyPropertyChangedEventArgs e)
    {
        IsVisibleChanged -= OnVisibleChangedShowTransaction;

        this.TransactionListBox.UpdateLayout();
        this.TransactionListBox.ScrollIntoView(Controller.ViewModel.SelectedRow);
    }

    private void SearchTextBox_OnKeyUp(object? sender, KeyEventArgs e)
    {
        this.ClearSearchButton.IsEnabled = this.SearchTextBox.Text.Length > 0;
    }
}
