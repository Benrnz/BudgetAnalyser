using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Ledger
{
    public class SpentMonthlyLedger : LedgerBucket
    {
        public override void ReconciliationBehaviour(IList<LedgerTransaction> transactions, DateTime reconciliationDate, decimal openingBalance)
        {
            decimal netAmount = transactions.Sum(t => t.Amount);
            decimal closingBalance = openingBalance + netAmount;
            BudgetCreditLedgerTransaction budgetTransaction = transactions.OfType<BudgetCreditLedgerTransaction>().FirstOrDefault();

            if (budgetTransaction == null)
            {
                transactions.AddIfSomething(SupplementToZero(closingBalance, reconciliationDate));

                return;
            }

            // Supplement
            if (closingBalance < budgetTransaction.Amount || closingBalance < openingBalance)
            {
                transactions.AddIfSomething(SupplementToBudgetAmount(closingBalance, reconciliationDate, budgetTransaction.Amount));
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

        public override void ValidateBucketSet(BudgetBucket bucket)
        {
            if (bucket is SpentMonthlyExpenseBucket)
            {
                return;
            }

            throw new NotSupportedException("Invalid budget bucket used, only Spent-Monthly-Expense-Bucket can be used with an instance of Spent-Monthly-Ledger.");
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