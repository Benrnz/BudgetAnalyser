using BudgetAnalyser.Engine.BankAccount;

namespace BudgetAnalyser.Statement;

/// <summary>
///     Interaction logic for AccountUserControl.xaml
/// </summary>
public partial class AccountUserControl
{
    public AccountUserControl()
    {
        InitializeComponent();
    }

    public Account Account => (Account)DataContext;
    public string FriendlyAccountName => Account.ToString();
}
