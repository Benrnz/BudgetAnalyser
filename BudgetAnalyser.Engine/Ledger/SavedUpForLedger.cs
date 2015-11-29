using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Ledger
{
    public class SavedUpForLedger : LedgerBucket
    {
        public override void ReconciliationBehaviour(List<LedgerTransaction> newTransactions, DateTime reconciliationDate, decimal openingBalance)
        {
            LedgerTransaction zeroingTransaction = null;
            var netAmount = newTransactions.Sum(t => t.Amount);

            // This ledger can accumulate a balance but cannot be negative.
            decimal closingBalance = openingBalance + netAmount;
            LedgerTransaction budgetedAmount = newTransactions.FirstOrDefault(t => t is BudgetCreditLedgerTransaction);
            if (budgetedAmount != null && closingBalance < budgetedAmount.Amount)
            {
                // This ledger has a monthly budgeted amount and the balance has resulted in a balance less than the monthly budgeted amount, supplement from surplus to equal budgeted amount.
                // While there is a monthly amount the balance should not drop below this amount.
                zeroingTransaction = new CreditLedgerTransaction
                {
                    Date = reconciliationDate,
                    Amount = budgetedAmount.Amount - closingBalance,
                    Narrative = closingBalance < 0 ? SupplementOverdrawnText : SupplementLessThanBudgetText
                };
            }
            else if (closingBalance < 0)
            {
                zeroingTransaction = new CreditLedgerTransaction
                {
                    Date = reconciliationDate,
                    Amount = -closingBalance,
                    Narrative = SupplementOverdrawnText
                };
            }
            if (zeroingTransaction != null) newTransactions.Add(zeroingTransaction);
        }

        public override void ValidateBucketSet(BudgetBucket bucket)
        {
            if (bucket is SavedUpForExpenseBucket) return;

            throw new NotSupportedException("Invalid budget bucket used, only Saved-Up-For-Expense-Buckets can be used with an instance of Saved-Up-For-Ledger.");
        }
    }
}