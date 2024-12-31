using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using BudgetAnalyser.Engine.Ledger;

namespace BudgetAnalyser.LedgerBook
{
    /// <summary>
    ///     Interaction logic for LedgerTransactionUserControl.xaml
    /// </summary>
    public partial class LedgerTransactionUserControl : UserControl
    {
        public LedgerTransactionUserControl()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        public decimal? Credit => LedgerTransaction is null ? null : LedgerTransaction.Amount >= 0 ? LedgerTransaction.Amount : 0M;

        public decimal? Debit => LedgerTransaction is null ? null : LedgerTransaction.Amount < 0 ? -LedgerTransaction.Amount : 0M;

        private LedgerTransaction LedgerTransaction => (LedgerTransaction)DataContext;

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Credit is not null && Credit >= 0)
            {
                this.CreditAmount.Text = Credit.Value.ToString("N", CultureInfo.CurrentCulture);
                this.CreditAmount.Visibility = Visibility.Visible;
                this.CreditLabel.Visibility = Visibility.Visible;
                this.DebitAmount.Visibility = Visibility.Hidden;
                this.DebitLabel.Visibility = Visibility.Hidden;
            }

            if (Debit is not null && Debit > 0)
            {
                this.DebitAmount.Text = Debit.Value.ToString("N", CultureInfo.CurrentCulture);
                this.CreditAmount.Visibility = Visibility.Hidden;
                this.CreditLabel.Visibility = Visibility.Hidden;
                this.DebitAmount.Visibility = Visibility.Visible;
                this.DebitLabel.Visibility = Visibility.Visible;
            }
        }
    }
}
