using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.UnitTest.TestHarness;

namespace BudgetAnalyser.UnitTest.Services
{
    public class StatementModelTestHarness : StatementModel
    {
        public StatementModelTestHarness() : base(new FakeLogger())
        {
        }

        public int FilterByCriteriaWasCalled { get; set; }
        public int FilterByTextWasCalled { get; set; }
        public int MergeWasCalled { get; set; }
        public int RemoveTransactionWasCalled { get; set; }
        public int SplitTransactionWasCalled { get; set; }

        internal override void Filter(GlobalFilterCriteria criteria)
        {
            FilterByCriteriaWasCalled++;
        }

        internal override void FilterByText(string textFilter)
        {
            FilterByTextWasCalled++;
        }

        internal override void Merge(StatementModel additionalModel)
        {
            MergeWasCalled++;
        }

        internal override void RemoveTransaction(Transaction transaction)
        {
            RemoveTransactionWasCalled++;
        }

        internal override void SplitTransaction(Transaction originalTransaction, decimal splinterAmount1, decimal splinterAmount2, BudgetBucket splinterBucket1, BudgetBucket splinterBucket2)
        {
            SplitTransactionWasCalled++;
        }
    }
}