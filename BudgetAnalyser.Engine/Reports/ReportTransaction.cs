using System;

namespace BudgetAnalyser.Engine.Reports
{
    /// <summary>
    ///     Represents a simple stripped down transaction to use in reporting.
    /// </summary>
    public class ReportTransaction
    {
        /// <summary>
        ///     Gets or sets the transaction amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        ///     Gets or sets the transaction date.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        ///     Gets or sets the transaction narrative.
        /// </summary>
        public string Narrative { get; set; }
    }
}
