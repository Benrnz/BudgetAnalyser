using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Matching.Data;

namespace BudgetAnalyser.UnitTest.TestData
{
    public static class MatchingRulesTestDataGenerator
    {
        public static void ConvertToDomainAndGenerateCSharp(IEnumerable<MatchingRuleDto> dataRules)
        {
            var bucketRepo = new InMemoryBudgetBucketRepository(new DtoToBudgetBucketMapper(new BudgetBucketFactory()));

            var mapper = new MatchingRuleDataToDomainMapper(bucketRepo);

            List<MatchingRule> domainRules = dataRules.Select(mapper.Map).ToList();

            Console.WriteLine(@"
/// <summary> THIS CODE IS GENERATED  </summary>
[GeneratedCode(""MatchingRulesTestData.ConvertToDomainAndGenerateCSharp"", ""{0}"")]
public static IEnumerable<MatchingRule> TestData999()
{{
    return new List<MatchingRule>
        {{", DateTime.Now);

            foreach (MatchingRule rule in domainRules)
            {
                Console.WriteLine(@"
            new MatchingRule(BucketRepo)");
                Console.WriteLine(@"
                {");
                Console.WriteLine(@"
                    Amount = {0},", NullDecimalOrValue(rule.Amount));
                Console.WriteLine(@"
                    BucketCode = ""{0}"",", rule.BucketCode);
                Console.WriteLine(@"
                    Created = new DateTime({0}, {1}, {2}),", rule.Created.Year, rule.Created.Month, rule.Created.Day);
                Console.WriteLine(@"
                    Description = {0},", NullStringOrQuotedValue(rule.Description));
                Console.WriteLine(@"
                    LastMatch = {0},", NullDateTimeOrValue(rule.LastMatch));
                Console.WriteLine(@"
                    MatchCount = {0},", rule.MatchCount);
                Console.WriteLine(@"
                    Reference1 = {0},", NullStringOrQuotedValue(rule.Reference1));
                Console.WriteLine(@"
                    Reference2 = {0},", NullStringOrQuotedValue(rule.Reference2));
                Console.WriteLine(@"
                    Reference3 = {0},", NullStringOrQuotedValue(rule.Reference3));
                Console.WriteLine(@"
                    RuleId = new Guid(""{0}""),", rule.RuleId);
                Console.WriteLine(@"
                    TransactionType = {0},", NullStringOrQuotedValue(rule.TransactionType));


                Console.WriteLine(@"
                },"); // Close new Rule initialiser
            }


            Console.WriteLine(@"
        };"); // Close new List Initialiser
            Console.WriteLine(@"
    }"); // Close Method
        }

        private static string NullDateTimeOrValue(DateTime? date)
        {
            if (date == null)
            {
                return "null";
            }

            return String.Format("new DateTime({0}, {1}, {2})", date.Value.Year, date.Value.Month, date.Value.Day);
        }

        private static string NullDecimalOrValue(decimal? amount)
        {
            if (amount == null)
            {
                return "null";
            }

            return String.Format("{0}M", amount);
        }

        private static string NullStringOrQuotedValue(string description)
        {
            if (String.IsNullOrWhiteSpace(description))
            {
                return "null";
            }

            return String.Format("\"{0}\"", description);
        }
    }
}