using System;
using System.Collections.Generic;
using System.Text;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.UnitTest
{
    public static class PublicHolidaysTestGenerator
    {
        [UsedImplicitly]
        public static string Generate()
        {
            var builder = new StringBuilder();
            WriteFileHeader(builder);
            WriteMainBody(builder);
            WriteFileFooter(builder);
            return builder.ToString();
        }

        private static void WriteFileFooter(StringBuilder builder)
        {
            builder.AppendLine(@"
} // End namespace");
        }

        private static void WriteFileHeader(StringBuilder builder)
        {
            builder.AppendFormat(@"
// Generated code do not directly modify
// {0}
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace BudgetAnalyser.UnitTest
{{
", DateTime.Now);
        }

        private static void WriteMainBody(StringBuilder builder)
        {
            foreach (KeyValuePair<int, IEnumerable<DateTime>> annualHoliday in PublicHolidaysTestData.ExpectedHolidays)
            {
                WriteTestClassHeader(builder, annualHoliday);
                WriteTestClassBody(builder, annualHoliday.Key);
                WriteTestClassFooter(builder, annualHoliday.Key);
            }
        }

        private static void WriteTestClassBody(StringBuilder builder, int year)
        {
            builder.AppendFormat(@"
        [TestMethod]
        public void CorrectNumberOfHolidays()
        {{
            Assert.AreEqual(this.expectedHolidays.Count(), this.subject.Results.Count());
        }}

        [TestInitialize]
        public void Initialise()
        {{
            this.subject = new NewZealandPublicHolidaysTestHarness({0});
        }}

        [TestMethod]
        public void VerifyHolidays()
        {{
            this.subject.VerifyHolidays(this.expectedHolidays);
        }}
", year);
        }

        private static void WriteTestClassFooter(StringBuilder builder, int year)
        {
            builder.AppendFormat(@"
    }} // End Test class for {0}", year);
        }

        private static void WriteTestClassHeader(StringBuilder builder, KeyValuePair<int, IEnumerable<DateTime>> annualHoliday)
        {
            builder.AppendFormat(@"
    [TestClass]
    public class PublicHolidays{0}Test
    {{
        private const int Year = {0};
        private NewZealandPublicHolidaysTestHarness subject;
        private List<DateTime> expectedHolidays = new List<DateTime>
            {{
", annualHoliday.Key);

            foreach (DateTime holiday in annualHoliday.Value)
            {
                builder.AppendFormat(@"
                new DateTime({0}, {1}, {2}),
", holiday.Year, holiday.Month, holiday.Day);
            }
            builder.AppendLine(@"            };");
        }
    }
}