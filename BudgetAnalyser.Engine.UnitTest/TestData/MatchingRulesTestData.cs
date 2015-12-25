﻿using System.Collections.Generic;
using BudgetAnalyser.Engine.Matching.Data;
using BudgetAnalyser.Engine.UnitTest.Helper;

namespace BudgetAnalyser.Engine.UnitTest.TestData
{
    public static class MatchingRulesTestData
    {
        public static IEnumerable<MatchingRuleDto> RawTestData1()
        {
            return EmbeddedResourceHelper.ExtractXaml<List<MatchingRuleDto>>("BudgetAnalyser.UnitTest.TestData.MatchingRulesTestData.xml");
        }
    }
}