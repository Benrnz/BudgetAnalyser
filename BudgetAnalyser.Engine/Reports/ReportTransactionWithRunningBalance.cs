using System;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Reports
{
    /// <summary>
    ///     A simple transaction for reporting purposes with a running balance.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Reports.ReportTransaction" />
    public class ReportTransactionWithRunningBalance : ReportTransaction
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ReportTransactionWithRunningBalance" /> class.
        /// </summary>
        public ReportTransactionWithRunningBalance()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReportTransactionWithRunningBalance" /> class.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public ReportTransactionWithRunningBalance([NotNull] ReportTransaction transaction)
        {
            if (transaction is null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            Amount = transaction.Amount;
            Narrative = transaction.Narrative;
            Date = transaction.Date;
        }

        /// <summary>
        ///     Gets or sets the running balance.
        /// </summary>
        public decimal Balance { get; set; }
    }
}
