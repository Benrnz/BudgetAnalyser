using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Budget
{
    public static class BudgetBucketBindingSource
    {
        /// <summary>
        ///     For UI binding purposes.
        ///     Needs to be set prior to use.
        /// </summary>
        public static IBudgetBucketRepository BucketRepository { get; set; }
    }
}