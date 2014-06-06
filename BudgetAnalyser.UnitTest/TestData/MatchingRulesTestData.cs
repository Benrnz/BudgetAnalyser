using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Xaml;
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
    }
}