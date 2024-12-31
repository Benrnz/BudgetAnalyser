//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using BudgetAnalyser.Engine.Budget;
//using BudgetAnalyser.Engine.Budget.Data;
//using BudgetAnalyser.Engine.Matching;
//using BudgetAnalyser.Engine.Matching.Data;

//namespace BudgetAnalyser.Engine.UnitTest.TestData
//{
//    public static class MatchingRulesTestDataGenerator
//    {
//        public static void ConvertToDomainAndGenerateCSharp(IEnumerable<MatchingRuleDto> dataRules)
//        {
//            var bucketRepo = new InMemoryBudgetBucketRepository(new DtoToBudgetBucketMapper(), new DtoToFixedBudgetBucketMapper());

//            var mapper = new DtoToMatchingRuleMapper();

//            List<MatchingRule> domainRules = dataRules.Select(mapper.Map).ToList();

//            Debug.WriteLine(@"
///// <summary> THIS CODE IS GENERATED  </summary>
//[GeneratedCode(""MatchingRulesTestData.ConvertToDomainAndGenerateCSharp"", ""{0}"")]
//public static IEnumerable<MatchingRule> TestData999()
//{{
//    return new List<MatchingRule>
//        {{", DateTime.Now);

//            foreach (MatchingRule rule in domainRules)
//            {
//                Debug.WriteLine(@"
//            new MatchingRule(BucketRepo)");
//                Debug.WriteLine(@"
//                {");
//                Debug.WriteLine(@"
//                    Amount = {0},", NullDecimalOrValue(rule.Amount));
//                Debug.WriteLine(@"
//                    BucketCode = ""{0}"",", rule.BucketCode);
//                Debug.WriteLine(@"
//                    Created = new DateTime({0}, {1}, {2}),", rule.Created.Year, rule.Created.Month, rule.Created.Day);
//                Debug.WriteLine(@"
//                    Description = {0},", NullStringOrQuotedValue(rule.Description));
//                Debug.WriteLine(@"
//                    LastMatch = {0},", NullDateTimeOrValue(rule.LastMatch));
//                Debug.WriteLine(@"
//                    MatchCount = {0},", rule.MatchCount);
//                Debug.WriteLine(@"
//                    Reference1 = {0},", NullStringOrQuotedValue(rule.Reference1));
//                Debug.WriteLine(@"
//                    Reference2 = {0},", NullStringOrQuotedValue(rule.Reference2));
//                Debug.WriteLine(@"
//                    Reference3 = {0},", NullStringOrQuotedValue(rule.Reference3));
//                Debug.WriteLine(@"
//                    RuleId = new Guid(""{0}""),", rule.RuleId);
//                Debug.WriteLine(@"
//                    TransactionType = {0},", NullStringOrQuotedValue(rule.TransactionType));


//                Debug.WriteLine(@"
//                },"); // Close new Rule initialiser
//            }


//            Debug.WriteLine(@"
//        };"); // Close new List Initialiser
//            Debug.WriteLine(@"
//    }"); // Close Method
//        }

//        private static string NullDateTimeOrValue(DateTime? date)
//        {
//            if (date is null)
//            {
//                return "null";
//            }

//            return string.Format("new DateTime({0}, {1}, {2})", date.Value.Year, date.Value.Month, date.Value.Day);
//        }

//        private static string NullDecimalOrValue(decimal? amount)
//        {
//            if (amount is null)
//            {
//                return "null";
//            }

//            return string.Format("{0}M", amount);
//        }

//        private static string NullStringOrQuotedValue(string description)
//        {
//            if (string.IsNullOrWhiteSpace(description))
//            {
//                return "null";
//            }

//            return string.Format("\"{0}\"", description);
//        }
//    }
//}
