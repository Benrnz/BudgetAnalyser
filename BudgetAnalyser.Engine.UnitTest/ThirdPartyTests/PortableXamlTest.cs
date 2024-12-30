using Portable.Xaml;

namespace BudgetAnalyser.Engine.UnitTest.ThirdPartyTests
{
    [TestClass]
    public class PortableXamlTest
    {
        [TestMethod]
        public void XamlServicesSave_ShouldSerialiseToISO8601_GivenUtcDateTime()
        {
            var testData = new TestDateTimeDto { Created = new DateTime(2015, 12, 30, 23, 50, 51, DateTimeKind.Utc) };
            var result = XamlServices.Save(testData);
            Console.WriteLine(result);
            Assert.IsTrue(result.Contains("2015-12-30T23:50:51Z"));
        }

        [TestMethod]
        public void XamlServicesSave_ShouldSerialiseToISO8601_GivenNullableUtcDateTime()
        {
            var result = XamlServices.Save(new TestDateTimeDto { NullableCreated = new DateTime(2016, 2, 29, 23, 55, 59, DateTimeKind.Utc) });
            Console.WriteLine(result);
            Assert.IsTrue(result.Contains("2016-02-29T23:55:59Z"));
        }

        [TestMethod]
        public void XamlServicesSave_ShouldSerialiseToISO8601_GivenDate()
        {
            var result = XamlServices.Save(new DateTime(2010, 4, 14));
            Console.WriteLine(result);
            Assert.IsTrue(result.Contains("2010-04-14"));
        }

        [TestMethod]
        public void XamlServicesSave_ShouldSerialiseToISO8601_GivenUtcDateTimeWithMilliseconds()
        {
            var testData = new TestDateTimeDto { Created = new DateTime(2015, 12, 30, 23, 50, 51, DateTimeKind.Utc) };
            testData.Created = testData.Created.AddMilliseconds(11);
            var result = XamlServices.Save(testData);
            Console.WriteLine(result);
            Assert.IsTrue(result.Contains("2015-12-30T23:50:51.011Z"));
        }

        [TestMethod]
        public void XamlServicesSave_ShouldSerialiseToISO8601_GivenLocalisedDateTimeWithMilliseconds()
        {
            var usTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var localisedDateTime = TimeZoneInfo.ConvertTimeFromUtc(
                new DateTime(2015, 12, 30, 23, 50, 51, DateTimeKind.Utc),
                usTimeZone);
            var testData = new TestDateTimeDto { Created = localisedDateTime };
            testData.Created = testData.Created.AddMilliseconds(11);
            var result = XamlServices.Save(testData);
            Console.WriteLine(result);
            Assert.IsTrue(result.Contains("2015-12-30T15:50:51.011"));
        }

        [TestMethod]
        public void XamlServicesSave_ShouldSerialiseToISO8601_GivenDateEndingWithZero()
        {
            var testData = new TestDateTimeDto { Created = new DateTime(2015, 12, 30) };
            var result = XamlServices.Save(testData);
            Console.WriteLine(result);
            Assert.IsTrue(result.Contains("2015-12-30"));
        }
    }

    public class TestDateTimeDto
    {
        public DateTime Created { get; set; }
        public DateTime? NullableCreated { get; set; }
    }
}