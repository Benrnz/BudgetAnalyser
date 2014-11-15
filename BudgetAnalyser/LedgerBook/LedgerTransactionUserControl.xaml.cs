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

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Credit != null && Credit >= 0)
            {
                CreditAmount.Text = Credit.Value.ToString("N", CultureInfo.CurrentCulture);
                CreditAmount.Visibility = Visibility.Visible;
                CreditLabel.Visibility = Visibility.Visible;
                DebitAmount.Visibility = Visibility.Hidden;
                DebitLabel.Visibility = Visibility.Hidden;
            }

            if (Debit != null && Debit > 0)
            {
                DebitAmount.Text = Debit.Value.ToString("N", CultureInfo.CurrentCulture);
                CreditAmount.Visibility = Visibility.Hidden;
                CreditLabel.Visibility = Visibility.Hidden;
                DebitAmount.Visibility = Visibility.Visible;
                DebitLabel.Visibility = Visibility.Visible;
            }
        }

        private LedgerTransaction LedgerTransaction
        {
            get { return (LedgerTransaction)DataContext; }
        }

        public decimal? Credit
        {
            get
            {
                if (LedgerTransaction == null) return null;
                return LedgerTransaction.Amount >= 0 ? LedgerTransaction.Amount : 0M;
            }
        }

        public decimal? Debit
        {
            get
            {
                if (LedgerTransaction == null) return null;
                return LedgerTransaction.Amount < 0 ? -LedgerTransaction.Amount : 0M;
            }
        }
    }
}