using System.Collections;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Ledger
{
    public class Ledger 
    {
        public BudgetBucket BudgetBucket { get; internal set; }

        public override int GetHashCode()
        {
            var hash = BudgetBucket.Code.GetHashCode();
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var otherLedger = obj as Ledger;
            if (otherLedger == null)
            {
                return false;
            }

            return BudgetBucket.Code.Equals(otherLedger.BudgetBucket.Code);
        }

    }
}