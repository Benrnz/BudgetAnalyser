using System;
using System.Globalization;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Ledger
{
    public class BudgetCreditLedgerTransaction : LedgerTransaction
    {
        public BudgetCreditLedgerTransaction() : base()
        {
        }

        public BudgetCreditLedgerTransaction(Guid id)
            : base(id)
        {
        }

        internal LedgerTransaction CreditRegularBudgettedAmount([NotNull] Expense budgettedExpense)
        {
            if (budgettedExpense == null)
            {
                throw new ArgumentNullException("budgettedExpense");
            }
            
            if (!(budgettedExpense.Bucket is SavedUpForExpense))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Given budgetted expense {0} is not a {1}.", GetType().Name, typeof(SavedUpForExpense).Name));
            }

            return WithNarrative(budgettedExpense.Bucket.ToString()).WithAmount(budgettedExpense.Amount);
        }
    }
}