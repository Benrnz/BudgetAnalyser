using Rees.Wpf;

namespace BudgetAnalyser;

public class MainMenuTabRequestMessage(MainMenuTab tab) : MessageBase
{
    public MainMenuTab Tab { get; } = tab;
}

public enum MainMenuTab { Dashboard, Transactions, Budget, Ledger, Reports }
