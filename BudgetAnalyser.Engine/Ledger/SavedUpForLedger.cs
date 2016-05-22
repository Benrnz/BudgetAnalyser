using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     A Ledger Bucket that allows funds to accumulate from month to month. Only spending or Ledger Book Transfers will
    ///     remove funds from this ledger.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Ledger.LedgerBucket" />
    public class SavedUpForLedger : LedgerBucket
    {
        /// <summary>
        ///     Allows ledger bucket specific behaviour during reconciliation.
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        public override void ApplyReconciliationBehaviour(IList<LedgerTransaction> transactions, DateTime reconciliationDate,
                                                          decimal openingBalance)
        {
            if (transactions == null)
            {
                throw new ArgumentNullException(nameof(transactions));
            }

            LedgerTransaction zeroingTransaction = null;
            var netAmount = transactions.Sum(t => t.Amount);

            // This ledger can accumulate a balance but cannot be negative.
            var closingBalance = openingBalance + netAmount;
            var budgetedAmount = transactions.FirstOrDefault(t => t is BudgetCreditLedgerTransaction);
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
            if (zeroingTransaction != null)
            {
                transactions.Add(zeroingTransaction);
            }
        }

        /// <summary>
        ///     Validates the bucket provided is valid for use with this LedgerBucket. There is an explicit relationship between
        ///     <see cref="BudgetBucket" />s and <see cref="LedgerBucket" />s.
        /// </summary>
        /// <exception cref="System.NotSupportedException">
        ///     Invalid budget bucket used, only Saved-Up-For-Expense-Buckets or
        ///     Savings-Commitment-Buckets can be used with an instance of Saved-Up-For-Ledger.
        /// </exception>
        protected override void ValidateBucketSet(BudgetBucket bucket)
        {
            if (bucket is SavedUpForExpenseBucket)
            {
                return;
            }
            if (bucket is SavingsCommitmentBucket)
            {
                return;
            }

            throw new NotSupportedException(
                "Invalid budget bucket used, only Saved-Up-For-Expense-Buckets or Savings-Commitment-Buckets can be used with an instance of Saved-Up-For-Ledger.");
        }
    }
}