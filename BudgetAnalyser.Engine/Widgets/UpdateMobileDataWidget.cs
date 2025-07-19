using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Widgets;

/// <summary>
///     Exports summarised data from Ledger and Transactions to a file and uploads it to web storage.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Widgets.Widget" />
[UsedImplicitly] // Instantiated by Widget Service / Repo
public sealed class UpdateMobileDataWidget : Widget
{
    private const string WidgetLabel = "Upload mobile data";

    private bool lockActive;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CurrentFileWidget" /> class.
    /// </summary>
    public UpdateMobileDataWidget()
    {
        Category = WidgetGroup.PeriodicTrackingSectionName;
        Dependencies = [typeof(LedgerBook), typeof(TransactionSetModel), typeof(BudgetCollection), typeof(GlobalFilterCriteria)];
        DetailedText = WidgetLabel;
        Sequence = 10;
        Clickable = true;
        Enabled = false;
        ImageResourceName = "MobileImage";
    }

    /// <summary>
    ///     The current Budget Collection held by this widget
    /// </summary>
    public BudgetCollection? BudgetCollection { get; private set; }

    /// <summary>
    ///     The current filter as held by this widget
    /// </summary>
    public GlobalFilterCriteria? Filter { get; private set; }

    /// <summary>
    ///     The current Ledger Book held by this widget
    /// </summary>
    public LedgerBook? LedgerBook { get; private set; }

    /// <summary>
    ///     The current Statement Model held by this widget
    /// </summary>
    public TransactionSetModel? TransactionsModel { get; private set; }

    /// <summary>
    ///     This method is used to disable the widget while upload is active.
    /// </summary>
    public void LockWhileUploading(bool @lock)
    {
        this.lockActive = @lock;
        Enabled = !this.lockActive;
        DetailedText = this.lockActive ? "Uploading..." : WidgetLabel;
    }

    /// <summary>
    ///     Updates the widget with new input.
    /// </summary>
    public override void Update(params object[] input)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        Enabled = false;

        if (!ValidateUpdateInput(input))
        {
            return;
        }

        LedgerBook = (LedgerBook)input[0];
        TransactionsModel = (TransactionSetModel)input[1];
        BudgetCollection = (BudgetCollection)input[2];
        Filter = (GlobalFilterCriteria)input[3];

        if (LedgerBook is null || TransactionsModel is null || BudgetCollection is null || BudgetCollection.CurrentActiveBudget is null || Filter is null)
        {
            return;
        }

        Enabled = true;
    }
}
