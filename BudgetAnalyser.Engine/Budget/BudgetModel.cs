using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace BudgetAnalyser.Engine.Budget
{
    /// <summary>
    ///     A Budget model that contains all budgeting information. A budget is effective for a period of time after the
    ///     <see cref="EffectiveFrom" />.
    /// </summary>
    public class BudgetModel
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BudgetModel" /> class.
        /// </summary>
        public BudgetModel()
        {
            Incomes = new List<Income>();
            Expenses = new List<Expense>();
            LastModified = DateTime.Now;
            // Set this here because the deserialisation process will reset if a value exists in the XML file. If not its better to have a date than min value.
            EffectiveFrom = DateTime.Now;
        }

        /// <summary>
        ///     Gets or sets the effective from date.
        /// </summary>
        public DateTime EffectiveFrom { get; set; }

        /// <summary>
        ///     Gets the expenses collection.
        /// </summary>
        public virtual IEnumerable<Expense> Expenses { get; private set; }

        /// <summary>
        ///     Gets the incomes collection.
        /// </summary>
        public virtual IEnumerable<Income> Incomes { get; private set; }

        /// <summary>
        ///     Gets the date and time the budget model was last modified by the user.
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        ///     Gets an optional comment than can be given when a change is made to the budget model.
        /// </summary>
        public string LastModifiedComment { get; set; }

        /// <summary>
        ///     Gets or sets the budget name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets the calculated surplus amount.
        /// </summary>
        public decimal Surplus => Incomes.Sum(i => i.Amount) - Expenses.Sum(e => e.Amount);

        /// <summary>
        ///     Initialises this instance ready for use. This should be done after loading the model from persistence.
        /// </summary>
        /// <exception cref="ValidationWarningException">The model is invalid.  + builder</exception>
        protected virtual void Initialise()
        {
            Expenses = Expenses.OrderByDescending(e => e.Amount).ToList();
            Incomes = Incomes.OrderByDescending(i => i.Amount).ToList();

            var builder = new StringBuilder();
            if (!Validate(builder))
            {
                // Consumer should have already called validate and resolved any issues before calling Update+Initialise.
                throw new ValidationWarningException("The model is invalid. " + builder);
            }
        }

        internal void Update(IEnumerable<Income> incomes, IEnumerable<Expense> expenses)
        {
            Incomes = incomes.ToList();
            Expenses = expenses.ToList();
            LastModified = DateTime.Now;
            Initialise();
        }

        internal virtual bool Validate(StringBuilder validationMessages)
        {
            var retval = true;
            retval &= Incomes.OfType<IModelValidate>().ToList().All(i => i.Validate(validationMessages));
            retval &= Expenses.OfType<IModelValidate>().ToList().All(e => e.Validate(validationMessages));

            if (Expenses.Any(e => e.Bucket.Code == SurplusBucket.SurplusCode))
            {
                validationMessages.AppendFormat(CultureInfo.CurrentCulture,
                    "You can not use SURPLUS as an expense code.");
                retval = false;
            }

            IEnumerable<string> duplicates = Expenses
                .GroupBy(i => i.Bucket.Code)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            foreach (var duplicateCode in duplicates)
            {
                retval = false;
                validationMessages.AppendFormat(CultureInfo.CurrentCulture,
                    "Expense {0} is listed multiple time, each bucket must have a different name.", duplicateCode);
            }

            return retval;
        }
    }
}