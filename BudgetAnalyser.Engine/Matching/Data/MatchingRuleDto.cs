using System;

namespace BudgetAnalyser.Engine.Matching.Data
{
    /// <summary>
    ///     A Dto object to persist a matching rule.
    /// </summary>
    public class MatchingRuleDto
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MatchingRuleDto" /> class.
        /// </summary>
        public MatchingRuleDto()
        {
            RuleId = Guid.NewGuid();
        }

        /// <summary>
        ///     Gets or sets the amount criteria.
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether if criteria is 'And'ed or 'Or'ed.
        /// </summary>
        public bool And { get; set; }

        /// <summary>
        ///     Gets or sets the bucket code to assign if matched.
        /// </summary>
        public string BucketCode { get; set; }

        /// <summary>
        ///     Gets or sets the created date.
        /// </summary>
        public DateTime? Created { get; set; }

        /// <summary>
        ///     Gets or sets the description criteria.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the last match date.
        /// </summary>
        public DateTime? LastMatch { get; set; }

        /// <summary>
        ///     Gets or sets the number of times this rule has matched a transaction.
        /// </summary>
        public int MatchCount { get; set; }

        /// <summary>
        ///     Gets or sets the reference1 criteria.
        /// </summary>
        public string Reference1 { get; set; }

        /// <summary>
        ///     Gets or sets the reference2 criteria.
        /// </summary>
        public string Reference2 { get; set; }

        /// <summary>
        ///     Gets or sets the reference3 criteria.
        /// </summary>
        public string Reference3 { get; set; }

        /// <summary>
        ///     Gets or sets the rule identifier.
        /// </summary>
        public Guid? RuleId { get; set; }

        /// <summary>
        ///     Gets or sets the transaction type criteria.
        /// </summary>
        public string TransactionType { get; set; }
    }
}