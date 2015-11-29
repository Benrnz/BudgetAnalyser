using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Ledger
{
    public class SpentMonthlyLedger : LedgerBucket
    {
        public override void ValidateBucketSet(BudgetBucket bucket)
        {
            if (bucket is SpentMonthlyExpenseBucket) return;

            throw new NotSupportedException("Invalid budget bucket used, only Spent-Monthly-Expense-Bucket can be used with an instance of Spent-Monthly-Ledger.");
        }

        public override void ReconciliationBehaviour(List<LedgerTransaction> newTransactions, DateTime reconciliationDate, decimal openingBalance)
        {
            //const string SupplementOverdrawnText = "Automatically supplementing overdrawn balance from surplus";
            //this.transactions = newTransactions.OrderBy(t => t.Date).ToList();
            //LedgerTransaction zeroingTransaction = null;
            //if (LedgerBucket.BudgetBucket is SpentMonthlyExpenseBucket && NetAmount != 0)
            //{
            //    // SpentMonthly ledgers automatically zero their balance. They dont accumulate nor can they be negative.
            //    // The balance does not need to be updated, it will always remain the same as the previous closing balance.
            //    if (NetAmount < 0)
            //    {
            //        BudgetCreditLedgerTransaction budgetAmountTransaction = newTransactions.OfType<BudgetCreditLedgerTransaction>().FirstOrDefault();
            //        if (budgetAmountTransaction != null)
            //        {
            //            // Ledger is still overdrawn despite having a budgeted amount transfered into this ledger bucket. Create a zeroing transaction so the sum total of all txns = 0.
            //            // This way the ledger closing balance will be equal to the previous ledger closing balance.
            //            decimal adjustmentAmount = -NetAmount;
            //            if (adjustmentAmount + Balance < budgetAmountTransaction.Amount)
            //            {
            //                adjustmentAmount = budgetAmountTransaction.Amount - (Balance + newTransactions.Sum(t => t.Amount));
            //                Balance = budgetAmountTransaction.Amount;
            //            }
            //            zeroingTransaction = new CreditLedgerTransaction
            //            {
            //                Date = reconciliationDate,
            //                Amount = adjustmentAmount,
            //                Narrative = "SpentMonthlyLedger: " + SupplementOverdrawnText
            //            };
            //        }
            //        else
            //        {
            //            if (Balance + NetAmount < 0)
            //            {
            //                // This ledger does not have a monthly budgeted amount, so if all funds are gone, it must be zeroed.
            //                zeroingTransaction = new CreditLedgerTransaction
            //                {
            //                    Date = reconciliationDate,
            //                    Amount = -(Balance + NetAmount),
            //                    Narrative = "SpentMonthlyLedger: " + SupplementOverdrawnText
            //                };
            //                Balance = 0;
            //            }
            //            else
            //            {
            //                // Some of the funds accumulated in this ledger have been spent, but a positive closing balance remains.
            //                Balance += NetAmount;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        zeroingTransaction = new CreditLedgerTransaction
            //        {
            //            Date = reconciliationDate,
            //            Amount = -NetAmount,
            //            Narrative = "SpentMonthlyLedger: automatically zeroing the credit remainder"
            //        };
            //    }
            //    if (zeroingTransaction != null)
            //    {
            //        this.transactions.Add(zeroingTransaction);
            //    }
            //}
            //else
            //{
            //    // All other ledgers can accumulate a balance but cannot be negative.
            //    decimal newBalance = Balance + NetAmount;
            //    LedgerTransaction budgetedAmount = this.transactions.FirstOrDefault(t => t is BudgetCreditLedgerTransaction);
            //    if (budgetedAmount != null && newBalance < budgetedAmount.Amount)
            //    {
            //        // This ledger has a monthly budgeted amount and the balance has resulted in a balance less than the monthly budgeted amount, supplement from surplus to equal budgeted amount.
            //        // While there is a monthly amount the balance should not drop below this amount.
            //        zeroingTransaction = new CreditLedgerTransaction
            //        {
            //            Date = reconciliationDate,
            //            Amount = budgetedAmount.Amount - newBalance,
            //            Narrative = newBalance < 0 ? SupplementOverdrawnText : "Automatically supplementing shortfall so balance is not less than monthly budget amount"
            //        };
            //        this.transactions.Add(zeroingTransaction);
            //        Balance += NetAmount;
            //    }
            //    else if (newBalance < 0)
            //    {
            //        zeroingTransaction = new CreditLedgerTransaction
            //        {
            //            Date = reconciliationDate,
            //            Amount = -newBalance,
            //            Narrative = SupplementOverdrawnText
            //        };
            //        this.transactions.Add(zeroingTransaction);
            //        Balance = 0;
            //    }
            //    else
            //    {
            //        Balance = newBalance;
            //    }
            //}
        }
    }
}