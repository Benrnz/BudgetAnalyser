namespace BudgetAnalyser.Engine.Budget.Data
{
    /// <summary>
    ///     A Dto to persist a Budget Expense object.
    /// </summary>
    public class ExpenseDto
    {
        /// <summary>
        ///     Gets or sets the amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        ///     Gets or sets the textual budget bucket code.
        /// </summary>
        public string BudgetBucketCode { get; set; }
    }
}