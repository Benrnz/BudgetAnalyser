using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook;

public class LedgerBucketUpdatedMessage(bool wasChanged) : MessageBase
{
    public bool WasChanged { get; private set; } = wasChanged;
}
