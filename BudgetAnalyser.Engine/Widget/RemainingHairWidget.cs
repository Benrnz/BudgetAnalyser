using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Ledger;

namespace BudgetAnalyser.Engine.Widget
{
    public class RemainingHairWidget : RemainingBudgetBucketWidget
    {
        private LedgerBook ledgerBook;

        public RemainingHairWidget()
        {
            DetailedText = "Hair cuts / beauty";
            DependencyMissingToolTip = "A Statement, Budget, or a Filter are not present, remaining haircut budget cannot be calculated.";
            RemainingBudgetToolTip = "Remaining Hair cut budget for period is {0:C}";
            BucketCode = "HAIRCUT";
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