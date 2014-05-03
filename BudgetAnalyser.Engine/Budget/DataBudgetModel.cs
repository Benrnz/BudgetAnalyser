using System;
using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Budget
{
    public class DataBudgetModel
    {
        public DateTime EffectiveFrom { get; set; }

        /// <summary>
        /// No need for a data type for <see cref="Income"/>, <see cref="Expenses"/>, <see cref="BudgetItem"/>, 
        /// as these have no properties that need to be private.
        /// </summary>
        public List<Expense> Expenses { get; set; }

        /// <summary>
        /// No need for a data type for <see cref="Income"/>, <see cref="Expenses"/>, <see cref="BudgetItem"/>, 
        /// as these have no properties that need to be private.
        /// </summary>
        public List<Income> Incomes { get; set; }

        /// <summary>
        ///     Gets the date and time the budget model was last modified by the user.
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        ///     Gets an optional comment than can be given when a change is made to the budget model.
        /// </summary>
        public string LastModifiedComment { get; set; }

        public string Name { get; set; }
    }
}
