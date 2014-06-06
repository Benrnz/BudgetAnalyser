using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Xaml;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;

namespace BudgetAnalyser.UnitTest.TestData
{
    public static class MatchingRulesTestData
    {
        public static IEnumerable<DataMatchingRule> RawTestData1()
        {
            // this line of code is useful to figure out the name Vs has given the resource! The name is case sensitive.
            Assembly.GetExecutingAssembly().GetManifestResourceNames().ToList().ForEach(n => Debug.WriteLine(n));

            string fileName = "BudgetAnalyser.UnitTest.TestData.MatchingRulesTestData.xml";
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName))
            {
                if (stream == null)
                {
                    throw new MissingManifestResourceException("Cannot find resource named: " + fileName);
                }

                return (List<DataMatchingRule>)XamlServices.Load(new XamlXmlReader(stream));
            }
        }

        public static void ConvertToDomainAndGenerateCSharp(IEnumerable<DataMatchingRule> dataRules)
        {
            var bucketRepo = new InMemoryBudgetBucketRepository();

            var mapper = new MatchingRuleDataToDomainMapper(bucketRepo);

            var domainRules = dataRules.Select(mapper.Map).ToList();
                    
            Console.WriteLine(@"
/// <summary> THIS CODE IS GENERATED  </summary>
[GeneratedCode(""MatchingRulesTestData.ConvertToDomainAndGenerateCSharp"", ""{0}"")]
public static IEnumerable<MatchingRule> TestData999()
{{
    return new List<MatchingRule>
        {{", DateTime.Now);

            foreach (var rule in domainRules)
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
                },");  // Close new Rule initialiser
            }


            Console.WriteLine(@"
        };"); // Close new List Initialiser
            Console.WriteLine(@"
    }"); // Close Method
        }

        private static string NullStringOrQuotedValue(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return "null";
            }

            return string.Format("\"{0}\"", description);
        }

        private static string NullDateTimeOrValue(DateTime? date)
        {
            if (date == null)
            {
                return "null";
            }

            return string.Format("new DateTime({0}, {1}, {2})", date.Value.Year, date.Value.Month, date.Value.Day);
        }

        private static string NullDecimalOrValue(decimal? amount)
        {
            if (amount == null)
            {
                return "null";
            }

            return string.Format("{0}M", amount);
        }
    }
}
