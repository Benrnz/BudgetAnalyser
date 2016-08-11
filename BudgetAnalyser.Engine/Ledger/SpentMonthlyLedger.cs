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
    public class SpentMonthlyLedger : LedgerBucket
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
        public override void ApplyReconciliationBehaviour(IList<LedgerTransaction> transactions, DateTime reconciliationDate, decimal openingBalance)
        {
            var netAmount = transactions.Sum(t => t.Amount);
            var closingBalance = openingBalance + netAmount;
            var budgetTransaction = transactions.OfType<BudgetCreditLedgerTransaction>().FirstOrDefault();

            if (budgetTransaction == null)
            {
                transactions.AddIfSomething(SupplementToZero(closingBalance, reconciliationDate));

                return;
            }

            // Supplement
            if (closingBalance < budgetTransaction.Amount)
            {
                transactions.AddIfSomething(SupplementToBudgetAmount(closingBalance, reconciliationDate, budgetTransaction.Amount));
                return;
            }

            if (closingBalance < openingBalance)
            {
                transactions.AddIfSomething(SupplementToOpeningBalance(closingBalance, reconciliationDate, openingBalance));
                return;
            }

            // Remove-excess
            if (closingBalance > openingBalance || closingBalance > budgetTransaction.Amount)
            {
                if (openingBalance > budgetTransaction.Amount)
                {
                    transactions.AddIfSomething(RemoveExcessToOpeningBalance(closingBalance, reconciliationDate, openingBalance));
                }
                else
                {
                    transactions.AddIfSomething(RemoveExcessToBudgetAmount(closingBalance, reconciliationDate, budgetTransaction.Amount));
                }
            }
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
            if (bucket is SpentMonthlyExpenseBucket)
            {
                return;
            }

            throw new NotSupportedException(
                "Invalid budget bucket used, only Spent-Monthly-Expense-Bucket can be used with an instance of Spent-Monthly-Ledger.");
        }

        private static LedgerTransaction RemoveExcessToBudgetAmount(decimal closingBalance, DateTime reconciliationDate, decimal budgetAmount)
        {
            if (closingBalance - budgetAmount == 0)
            {
                return null;
            }
            return new CreditLedgerTransaction
            {
                Amount = -(closingBalance - budgetAmount),
                Date = reconciliationDate,
                Narrative = RemoveExcessText
            };
        }

        private static LedgerTransaction RemoveExcessToOpeningBalance(decimal closingBalance, DateTime reconciliationDate, decimal openingBalance)
        {
            if (closingBalance - openingBalance == 0)
            {
                return null;
            }
            return new CreditLedgerTransaction
            {
                Amount = -(closingBalance - openingBalance),
                Date = reconciliationDate,
                Narrative = RemoveExcessText
            };
        }

        private static LedgerTransaction SupplementToBudgetAmount(decimal closingBalance, DateTime reconciliationDate, decimal budgetAmount)
        {
            if (budgetAmount - closingBalance == 0)
            {
                return null;
            }
            return new CreditLedgerTransaction
            {
                Amount = budgetAmount - closingBalance,
                Date = reconciliationDate,
                Narrative = SupplementLessThanBudgetText
            };
        }

        private static LedgerTransaction SupplementToOpeningBalance(decimal closingBalance, DateTime reconciliationDate, decimal openingBalance)
        {
            if (openingBalance - closingBalance == 0)
            {
                return null;
            }
            return new CreditLedgerTransaction
            {
                Amount = openingBalance - closingBalance,
                Date = reconciliationDate,
                Narrative = SupplementLessThanOpeningBalance
            };
        }

        private static CreditLedgerTransaction SupplementToZero(decimal closingBalance, DateTime reconciliationDate)
        {
            if (closingBalance == 0)
            {
                return null;
            }
            return new CreditLedgerTransaction
            {
                Amount = 0 - closingBalance,
                Date = reconciliationDate,
                Narrative = RemoveExcessNoBudgetAmountText
            };
        }
    }
}