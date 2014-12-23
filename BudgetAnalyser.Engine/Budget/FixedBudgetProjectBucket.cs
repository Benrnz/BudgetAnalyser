using System;
using System.Globalization;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget
{
    public class FixedBudgetProjectBucket : SurplusBucket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FixedBudgetProjectBucket"/> class.
        /// Used only for persistence.
        /// </summary>
        internal FixedBudgetProjectBucket()
        {
        }

        public FixedBudgetProjectBucket(string code, string description, decimal fixedBudgetAmount)
            : base(CreateCode(code), description)
        {
            FixedBudgetAmount = fixedBudgetAmount;
            Created = DateTime.Now;
        }

        public DateTime Created { get; private set; }

        public override string TypeDescription
        {
            get { return "Fixed Budget Project (A sub-category of Surplus)"; }
        }

        public decimal FixedBudgetAmount { get; private set; }

        public string SubCode
        {
            get { return Code.Substring(Code.IndexOf('.')+1); }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "[{0}] {1} {2}", Code, Description, Created.ToShortDateString());
        }

        public static string CreateCode([NotNull] string subCode)
        {
            if (subCode == null)
            {
                throw new ArgumentNullException("subCode");
            }

            return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", SurplusCode, subCode.ToUpperInvariant());
        }
    }
}