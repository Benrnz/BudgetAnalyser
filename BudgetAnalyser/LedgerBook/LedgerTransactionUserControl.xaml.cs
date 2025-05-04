using System.Globalization;
using System.Windows;
using BudgetAnalyser.Engine.Ledger;

namespace BudgetAnalyser.LedgerBook;

/// <summary>
///     Interaction logic for LedgerTransactionUserControl.xaml
/// </summary>
public partial class LedgerTransactionUserControl
{
    public LedgerTransactionUserControl()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    public decimal Credit => LedgerTransaction.Amount >= 0 ? LedgerTransaction.Amount : 0M;

    public decimal Debit => LedgerTransaction.Amount < 0 ? -LedgerTransaction.Amount : 0M;

    private LedgerTransaction LedgerTransaction => (LedgerTransaction)DataContext;

    private void OnDataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
    {
        if (Credit >= 0)
        {
            this.CreditAmount.Text = Credit.ToString("N", CultureInfo.CurrentCulture);
            this.CreditAmount.Visibility = Visibility.Visible;
            this.CreditLabel.Visibility = Visibility.Visible;
            this.DebitAmount.Visibility = Visibility.Hidden;
            this.DebitLabel.Visibility = Visibility.Hidden;
        }

        if (Debit > 0)
        {
            this.DebitAmount.Text = Debit.ToString("N", CultureInfo.CurrentCulture);
            this.CreditAmount.Visibility = Visibility.Hidden;
            this.CreditLabel.Visibility = Visibility.Hidden;
            this.DebitAmount.Visibility = Visibility.Visible;
            this.DebitLabel.Visibility = Visibility.Visible;
        }
    }
}
