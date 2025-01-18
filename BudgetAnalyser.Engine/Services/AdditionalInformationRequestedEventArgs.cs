namespace BudgetAnalyser.Engine.Services;

/// <summary>
///     A set or event arguments for the <see cref="INotifyDatabaseChanges.Saving" /> event.
///     This event is used to optionally gather information from all subscribers before Saving.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Services.ValidatingEventArgs" />
public class AdditionalInformationRequestedEventArgs : ValidatingEventArgs
{
    /// <summary>
    ///     Gets or sets the context. The consumer of the event can optionally set the context.
    /// </summary>
    public object? Context { get; set; }
}
