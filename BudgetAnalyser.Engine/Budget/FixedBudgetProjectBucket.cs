using System;
using System.Globalization;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Budget
{
    /// <summary>
    ///     A fixed budget project bucket. This is a specialisation of Surplus, so any funds classified with this bucket are
    ///     also considered surplus funds as well.
    ///     This is a bucket used to represent a large but short lived project that should not exceed a budgeted amount.  Using
    ///     this bucket helps track progress against a budgeted amount for the project.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Budget.SurplusBucket" />
    public class FixedBudgetProjectBucket : SurplusBucket
    {
        private const string ProjectCodeTemplate = "{0}.{1}";

        /// <summary>
        ///     The project code template used when creating a code. This is used to produce a <see cref="BudgetBucket.Code" />
        ///     that is prefixed with SURPLUS.
        /// </summary>
        public static readonly string ProjectCodeTemplateWithPrefix = string.Format(ProjectCodeTemplate, SurplusCode, "{0}");

        /// <summary>
        ///     Initializes a new instance of the <see cref="FixedBudgetProjectBucket" /> class.
        ///     Used to create a new instance from the User Interface.
        /// </summary>
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
        internal FixedBudgetProjectBucket(string code, string description, decimal fixedBudgetAmount, DateTime created)
            : base(code, description)
        {
            FixedBudgetAmount = fixedBudgetAmount;
            Created = created;
        }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        /// <summary>
        ///     Gets the created date.
        /// </summary>
        public DateTime Created { get; private set; }

        /// <summary>
        ///     Gets the fixed budget amount.
        /// </summary>
        public decimal FixedBudgetAmount { get; private set; }

        /// <summary>
        ///     Gets the sub code. This is the code the SURPLUS prefix.
        /// </summary>
        public string SubCode => Code.Substring(Code.IndexOf('.') + 1);

        /// <summary>
        ///     Gets a description of this type of bucket. By default this is the <see cref="System.Type.Name" />
        /// </summary>
        public override string TypeDescription => "Fixed Budget Project (A sub-category of Surplus)";

        /// <summary>
        ///     Creates the code.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public static string CreateCode([NotNull] string subCode)
        {
            if (subCode == null)
            {
                throw new ArgumentNullException(nameof(subCode));
            }

            return string.Format(CultureInfo.InvariantCulture, ProjectCodeTemplateWithPrefix, subCode.ToUpperInvariant());
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "[{0}] {1} {2}", Code, Description, Created.ToString("d"));
        }
    }
}