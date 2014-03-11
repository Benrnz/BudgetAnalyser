﻿using System;

namespace BudgetAnalyser.Engine.Budget
{
    public class SurplusBucket : BudgetBucket
    {
        public SurplusBucket()
        {
            Id = new Guid("dbaf34f9-5d8d-4984-8303-a022ab49b98a");
        }

        public SurplusBucket(string code, string description) : base(code, description)
        {
            Id = new Guid("dbaf34f9-5d8d-4984-8303-a022ab49b98a");
        }

        public override string TypeDescription
        {
            get { return "Calculated Surplus"; }
        }
    }
}