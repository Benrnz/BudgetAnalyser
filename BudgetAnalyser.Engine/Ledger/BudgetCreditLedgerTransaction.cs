using System;
using System.Globalization;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Ledger
{
    public class BudgetCreditLedgerTransaction : LedgerTransaction
    {
        public BudgetCreditLedgerTransaction()
        {
        }

        public BudgetCreditLedgerTransaction(Guid id)
            : base(id)
        {
        }

        internal LedgerTransaction CreditRegularBudgetedAmount([NotNull] Expense budgetedExpense)
        {
            if (budgetedExpense == null)
            {
                throw new ArgumentNullException("budgetedExpense");
            }

            if (!(budgetedExpense.Bucket is SavedUpForExpenseBucket))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Given budgeted expense {0} is not a {1}.", GetType().Name, typeof(SavedUpForExpenseBucket).Name));
            }

            return WithNarrative(budgetedExpense.Bucket.ToString()).WithAmount(budgetedExpense.Amount);
        }
    }
}