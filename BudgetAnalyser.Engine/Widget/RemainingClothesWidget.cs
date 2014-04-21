using System;
using System.Collections.Generic;
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
            return LedgerCalculation.LocateApplicableLedgerBalance(this.ledgerBook, Filter, BucketCode);
        }

        protected override void SetAdditionalDependencies(object[] input)
        {
            base.SetAdditionalDependencies(input);
            this.ledgerBook = (LedgerBook)input[4];
        }
    }
}