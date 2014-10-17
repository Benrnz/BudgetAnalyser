using System;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Reports
{
    public class ReportTransactionWithRunningBalance : ReportTransaction
    {
        public ReportTransactionWithRunningBalance()
        {
        }

        public ReportTransactionWithRunningBalance([NotNull] ReportTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            Amount = transaction.Amount;
            Narrative = transaction.Narrative;
            Date = transaction.Date;
        }

        public decimal Balance { get; set; }
    }
}