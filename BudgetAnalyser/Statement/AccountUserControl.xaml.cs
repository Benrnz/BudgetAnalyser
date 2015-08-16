using System.Windows.Controls;
using BudgetAnalyser.Engine.Account;

namespace BudgetAnalyser.Statement
{
    /// <summary>
    ///     Interaction logic for AccountUserControl.xaml
    /// </summary>
    public partial class AccountUserControl : UserControl
    {
        public AccountUserControl()
        {
            InitializeComponent();
        }

        public Account Account => (Account)DataContext;
        public string FriendlyAccountName => Account.ToString();
    }
}