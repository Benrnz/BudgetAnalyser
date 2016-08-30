using System;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     Exports summarised data from Ledger and Transactions to a file and uploads it to web storage.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Widgets.Widget" />
    public sealed class UpdateMobileDataWidget : Widget
    {
        private const string WidgetLabel = "Upload mobile data";

        /// <summary>
        ///     Initializes a new instance of the <see cref="CurrentFileWidget" /> class.
        /// </summary>
        public UpdateMobileDataWidget()
        {
            Category = WidgetGroup.MonthlyTrackingSectionName;
            Dependencies = new[] { typeof(LedgerBook), typeof(StatementModel), typeof(BudgetCollection), typeof(GlobalFilterCriteria) };
            DetailedText = WidgetLabel;
            Sequence = 10;
            Clickable = true;
            Enabled = false;
            ImageResourceName = "MobileImage";
        }

        /// <summary>
        ///     The current Budget Collection held by this widget
        /// </summary>
        public BudgetCollection BudgetCollection { get; private set; }

        /// <summary>
        ///     The current filter as held by this widget
        /// </summary>
        public GlobalFilterCriteria Filter { get; private set; }

        /// <summary>
        ///     The current Ledger Book held by this widget
        /// </summary>
        public LedgerBook LedgerBook { get; private set; }

        /// <summary>
        ///     The current Statement Model held by this widget
        /// </summary>
        public StatementModel StatementModel { get; private set; }

        private bool lockActive = false;

        /// <summary>
        /// This method is used to disable the widget while upload is active.
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
        /// <exception cref="System.ArgumentNullException"></exception>
        public override void Update([NotNull] params object[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            Enabled = false;

            if (!ValidateUpdateInput(input)) return;

            LedgerBook = (LedgerBook) input[0];
            StatementModel = (StatementModel) input[1];
            BudgetCollection = (BudgetCollection) input[2];
            Filter = (GlobalFilterCriteria) input[3];

            if (LedgerBook == null || StatementModel == null || BudgetCollection == null || Filter == null) return;

            Enabled = true;
        }
    }
}