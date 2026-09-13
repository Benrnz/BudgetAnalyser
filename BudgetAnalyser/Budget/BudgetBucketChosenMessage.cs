using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using Rees.Wpf;

namespace BudgetAnalyser.Budget;

/// <summary>
///     Broadcast when the choose budget bucket dialog completes.
/// </summary>
public class BudgetBucketChosenMessage : MessageBase
{
    public BudgetBucketChosenMessage(object? sender, Guid correlationId, bool canceled)
    {
        Sender = sender;
        CorrelationId = correlationId;
        Canceled = canceled;
    }

    public BudgetBucketChosenMessage(object? sender, Guid correlationId, BudgetBucket? bucket)
        : this(sender, correlationId, false)
    {
        SelectedBucket = bucket;
    }

    public BudgetBucketChosenMessage(object? sender, Guid correlationId, BudgetBucket? bucket, Account? storeInThisAccount)
        : this(sender, correlationId, bucket)
    {
        StoreInThisAccount = storeInThisAccount;
    }

    public bool Canceled { get; }

    public Guid CorrelationId { get; }

    public BudgetBucket? SelectedBucket { get; }

    public Account? StoreInThisAccount { get; }
}
