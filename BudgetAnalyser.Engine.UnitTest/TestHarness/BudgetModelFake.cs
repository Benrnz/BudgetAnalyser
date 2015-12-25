﻿using System;
using System.Text;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.UnitTest.TestHarness
{
    public class BudgetModelFake : BudgetModel
    {
        public Action InitialiseOverride { get; set; }
        public Func<StringBuilder, bool> ValidateOverride { get; set; }

        internal override bool Validate(StringBuilder validationMessages)
        {
            if (ValidateOverride != null)
            {
                return ValidateOverride(validationMessages);
            }

            return base.Validate(validationMessages);
        }

        protected override void Initialise()
        {
            if (InitialiseOverride != null)
            {
                InitialiseOverride();
                return;
            }

            base.Initialise();
        }
    }
}