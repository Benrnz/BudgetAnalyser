using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BudgetAnalyser.Engine.Ledger;

namespace BudgetAnalyser.Engine.Widget
{
    public class RemainingClothesWidget : RemainingBudgetBucketWidget
    {
        private LedgerBook ledgerBook;

        public RemainingClothesWidget()
        {
            DetailedText = "Clothes";
            DependencyMissingToolTip = "A Statement, Budget, or a Filter are not present, remaining clothes budget cannot be calculated.";
            RemainingBudgetToolTip = "Remaining Clothes budget for period is {0:C}";
            BucketCode = "CLOTHES";
            List<Type> baseDependencies = Dependencies.ToList();
            baseDependencies.Add(typeof(LedgerBook));
            Dependencies = baseDependencies.ToArray();
        }

        protected override decimal MonthlyBudgetAmount()
        {
            decimal monthlyBudget = base.MonthlyBudgetAmount();
            if (this.ledgerBook == null)
            {
                return monthlyBudget;
            }

            // Filter has already been checked for null and for cleared status.
            Debug.Assert(Filter.BeginDate != null);
            Debug.Assert(Filter.EndDate != null);
            LedgerEntryLine line = this.ledgerBook.DatedEntries.FirstOrDefault(ledgerEntryLine => ledgerEntryLine.Date >= Filter.BeginDate.Value && ledgerEntryLine.Date <= Filter.EndDate.Value);
            if (line == null)
            {
                return monthlyBudget;
            }

            decimal ledgerBalance = (from ledgerEntry in line.Entries where ledgerEntry.Ledger.BudgetBucket.Code == BucketCode select ledgerEntry.Balance).FirstOrDefault();
            return monthlyBudget + ledgerBalance;
        }

        protected override bool SetAdditionalDependencies(object[] input)
        {
            base.SetAdditionalDependencies(input);
            var newLedger = (LedgerBook)input[4];
            if (newLedger != this.ledgerBook)
            {
                this.ledgerBook = newLedger;
                return true;
            }

            return false;
        }
    }
}