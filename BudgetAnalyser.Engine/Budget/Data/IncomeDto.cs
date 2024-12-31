namespace BudgetAnalyser.Engine.Budget.Data
{
    /// <summary>
    ///     A Dto persistence class to store an Income object.
    /// </summary>
    public class IncomeDto
    {
        /// <summary>
        ///     Gets or sets the income amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        ///     Gets or sets the budget bucket textual code.
        /// </summary>
        public string BudgetBucketCode { get; set; }
    }
}
