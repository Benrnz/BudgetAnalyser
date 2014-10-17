using System.Windows.Controls;
using BudgetAnalyser.Engine.Account;

namespace BudgetAnalyser.Statement
{
    /// <summary>
    ///     Interaction logic for AccountTypeUserControl.xaml
    /// </summary>
    public partial class AccountTypeUserControl : UserControl
    {
        public AccountTypeUserControl()
        {
            InitializeComponent();
        }

        public AccountType Account
        {
            get { return (AccountType)DataContext; }
        }

        public string FriendlyAccountName
        {
            get { return Account.ToString(); }
        }
    }
}