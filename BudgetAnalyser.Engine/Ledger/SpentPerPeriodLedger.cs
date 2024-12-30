using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     A Ledger Bucket that does not allow funds to accumulate at the end of the month. Any excess funds if not spent,
    ///     will be transfered to Surplus.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Ledger.LedgerBucket" />
    public class SpentPerPeriodLedger : LedgerBucket
    {
        /// <summary>
        ///     A constant for the "remove excess and no budget amount" text
        /// </summary>
        private const string RemoveExcessNoBudgetAmountText = "Automatically removing excess funds down to zero given there is no budget amount for this ledger";

        /// <summary>
        ///     A constant for the "remove excess" text
        /// </summary>
        private const string RemoveExcessText = "Automatically removing excess funds.";

        private const string SupplementLessThanOpeningBalance = "Automatically supplementing shortfall so balance is not less than opening balance";

        /// <summary>
        ///     Allows ledger bucket specific behaviour during reconciliation.
        /// </summary>
        public override bool ApplyReconciliationBehaviour(IList<LedgerTransaction> transactions, DateTime reconciliationDate, decimal openingBalance)
        {
            var netAmount = transactions.Sum(t => t.Amount);
            var closingBalance = openingBalance + netAmount;
            var budgetTransaction = transactions.OfType<BudgetCreditLedgerTransaction>().FirstOrDefault();

            if (budgetTransaction is null)
            {
                return transactions.AddIfSomething(SupplementToZero(closingBalance, reconciliationDate));
            }

            // Supplement
            if (closingBalance < budgetTransaction.Amount)
            {
                return transactions.AddIfSomething(SupplementToBudgetAmount(closingBalance, reconciliationDate, budgetTransaction.Amount));
            }

            if (closingBalance < openingBalance)
            {
                return transactions.AddIfSomething(SupplementToOpeningBalance(closingBalance, reconciliationDate, openingBalance));
            }

            // Remove-excess
            if (closingBalance > openingBalance || closingBalance > budgetTransaction.Amount)
            {
                return openingBalance > budgetTransaction.Amount
                    ? transactions.AddIfSomething(RemoveExcessToOpeningBalance(closingBalance, reconciliationDate, openingBalance))
                    : transactions.AddIfSomething(RemoveExcessToBudgetAmount(closingBalance, reconciliationDate, budgetTransaction.Amount));
            }

            return false;
        }

        /// <summary>
        ///     Validates the bucket provided is valid for use with this LedgerBucket. There is an explicit relationship between
        ///     <see cref="BudgetBucket" />s and <see cref="LedgerBucket" />s.
        /// </summary>
        /// <exception cref="System.NotSupportedException">
        ///     Invalid budget bucket used, only Spent-Monthly-Expense-Bucket can be
        ///     used with an instance of Spent-Monthly-Ledger.
        /// </exception>
        protected override void ValidateBucketSet(BudgetBucket bucket)
        {
            if (bucket is SpentPerPeriodExpenseBucket)
            {
                return;
            }

            throw new NotSupportedException(
                "Invalid budget bucket used, only Spent-Monthly-Expense-Bucket can be used with an instance of Spent-Monthly-Ledger.");
        }

        private static LedgerTransaction RemoveExcessToBudgetAmount(decimal closingBalance, DateTime reconciliationDate, decimal budgetAmount)
        {
            return closingBalance - budgetAmount == 0
                ? null
                : (LedgerTransaction)new CreditLedgerTransaction
            {
                Amount = -(closingBalance - budgetAmount),
                Date = reconciliationDate,
                Narrative = RemoveExcessText
            };
        }

        private static LedgerTransaction RemoveExcessToOpeningBalance(decimal closingBalance, DateTime reconciliationDate, decimal openingBalance)
        {
            return closingBalance - openingBalance == 0
                ? null
                : (LedgerTransaction)new CreditLedgerTransaction
            {
                Amount = -(closingBalance - openingBalance),
                Date = reconciliationDate,
                Narrative = RemoveExcessText
            };
        }

        private static LedgerTransaction SupplementToBudgetAmount(decimal closingBalance, DateTime reconciliationDate, decimal budgetAmount)
        {
            return budgetAmount - closingBalance == 0
                ? null
                : (LedgerTransaction)new CreditLedgerTransaction
            {
                Amount = budgetAmount - closingBalance,
                Date = reconciliationDate,
                Narrative = SupplementLessThanBudgetText
            };
        }

        private static LedgerTransaction SupplementToOpeningBalance(decimal closingBalance, DateTime reconciliationDate, decimal openingBalance)
        {
            return openingBalance - closingBalance == 0
                ? null
                : (LedgerTransaction)new CreditLedgerTransaction
            {
                Amount = openingBalance - closingBalance,
                Date = reconciliationDate,
                Narrative = SupplementLessThanOpeningBalance
            };
        }

        private static CreditLedgerTransaction SupplementToZero(decimal closingBalance, DateTime reconciliationDate)
        {
            return closingBalance == 0
                ? null
                : new CreditLedgerTransaction
            {
                Amount = 0 - closingBalance,
                Date = reconciliationDate,
                Narrative = RemoveExcessNoBudgetAmountText
            };
        }
    }
}
