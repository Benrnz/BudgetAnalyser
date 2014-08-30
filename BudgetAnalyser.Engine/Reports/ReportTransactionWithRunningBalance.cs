namespace BudgetAnalyser.Engine.Reports
{
    public class ReportTransactionWithRunningBalance : ReportTransaction
    {
        public ReportTransactionWithRunningBalance()
        {
        }

        public ReportTransactionWithRunningBalance(ReportTransaction transaction)
        {
            Amount = transaction.Amount;
            Narrative = transaction.Narrative;
            Date = transaction.Date;
        }

        public decimal Balance { get; set; }
    }
}