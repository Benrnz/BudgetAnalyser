using System.Diagnostics;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Ledger
{
    [DebuggerDisplay("Ledger({BudgetBucket})")]
    public class Ledger
    {
        public BudgetBucket BudgetBucket { get; internal set; }

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

        public override int GetHashCode()
        {
            int hash = BudgetBucket.Code.GetHashCode();
            return hash;
        }
    }
}