using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook;

public class AddLedgerReconciliationCompletedMessage(bool canceled) : MessageBase
{
    public bool Canceled { get; private set; } = canceled;
}
