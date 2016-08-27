using System;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Persistence;
using JetBrains.Annotations;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     Exports summarised data from Ledger and Transactions to a file and uploads it to web storage.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Widgets.Widget" />
    public sealed class UpdateMobileDataWidget : Widget
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CurrentFileWidget" /> class.
        /// </summary>
        public UpdateMobileDataWidget()
        {
            Category = WidgetGroup.MonthlyTrackingSectionName;
            Dependencies = new[] { typeof(LedgerBook), typeof(StatementModel), typeof(IBudgetCurrencyContext) };
            Clickable = true;
            Enabled = false;
            ColourStyleName = WidgetStandardStyle;
            ImageResourceName = "LockOpenImage";
            Sequence = 11;
        }

        /// <summary>
        ///     Updates the widget with new input.
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        public override void Update([NotNull] params object[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (!ValidateUpdateInput(input)) return;

            var ledgerBook = (LedgerBook)input[0];
            var transactions = (StatementModel) input[1];
            var budgetContext = (IBudgetCurrencyContext) input[2];

        }
    }
}