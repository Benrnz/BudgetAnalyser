using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook;

public class LedgerTransactionsCompletedMessage(bool wasModified) : MessageBase
{
    public bool WasModified { get; private set; } = wasModified;
}
