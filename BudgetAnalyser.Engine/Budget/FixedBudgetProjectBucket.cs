using System;
using System.Globalization;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget
{
    public class FixedBudgetProjectBucket : SurplusBucket
    {
        public FixedBudgetProjectBucket(string code, string description, decimal fixedBudgetAmount)
            : base(CreateCode(code), description)
        {
            FixedBudgetAmount = fixedBudgetAmount;
            Created = DateTime.Now;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FixedBudgetProjectBucket" /> class.
        ///     Used only for persistence.
        /// </summary>
        internal FixedBudgetProjectBucket()
        {
        }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public DateTime Created { get; private set; }
        public decimal FixedBudgetAmount { get; private set; }
        public string SubCode => Code.Substring(Code.IndexOf('.') + 1);
        public override string TypeDescription => "Fixed Budget Project (A sub-category of Surplus)";

        public static string CreateCode([NotNull] string subCode)
        {
            if (subCode == null)
            {
                throw new ArgumentNullException(nameof(subCode));
            }

            return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", SurplusCode, subCode.ToUpperInvariant());
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "[{0}] {1} {2}", Code, Description, Created.ToShortDateString());
        }
    }
}