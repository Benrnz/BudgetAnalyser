using System;
using System.Text;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class BudgetModelFake : BudgetModel
    {
        public BudgetModelFake() : base(new FakeLogger())
        {
        }

        public Action InitialiseOverride { get; set; }

        public Func<StringBuilder, bool> ValidateOverride { get; set; }

        protected override void Initialise()
        {
            if (InitialiseOverride != null)
            {
                InitialiseOverride();
                return;
            }

            base.Initialise();
        }

        internal override bool Validate(StringBuilder validationMessages)
        {
            if (ValidateOverride != null)
            {
                return ValidateOverride(validationMessages);
            }

            return base.Validate(validationMessages);
        }
    }
}
