using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook;

public class AddLedgerReconciliationCompletedMessage(bool wasChanged) : MessageBase
{
    public bool WasChanged { get; private set; } = wasChanged;
}
