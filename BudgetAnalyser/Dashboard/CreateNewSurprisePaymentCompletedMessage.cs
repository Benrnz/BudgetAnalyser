using BudgetAnalyser.Engine.Widgets;
using Rees.Wpf;

namespace BudgetAnalyser.Dashboard;

public class CreateNewSurprisePaymentCompletedMessage(Guid correlationId, bool canceled) : MessageBase
{
    public string BucketCode { get; init; } = string.Empty;
    public bool Canceled { get; } = canceled;

    public Guid CorrelationId { get; } = correlationId;

    public WeeklyOrFortnightly Frequency { get; init; }

    public DateOnly PaymentStartDate { get; init; }
}
