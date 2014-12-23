using BudgetAnalyser.Engine;
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

        internal override void Filter(GlobalFilterCriteria criteria)
        {
            FilterByCriteriaWasCalled++;
        }

        internal override void FilterByText(string textFilter)
        {
            FilterByTextWasCalled++;
        }
    }
}