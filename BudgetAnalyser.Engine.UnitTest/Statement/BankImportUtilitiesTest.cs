using System;
using System.Globalization;
using System.IO;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Castle.Core.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.Engine.UnitTest.Statement
{
    [TestClass]
    public class BankImportUtilitiesTest
    {
        private Mock<IBudgetBucketRepository> BucketRepositoryMock { get; set; }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedIndexException))]
        public void FetchBudgetBucketWithNegativeOutOfRangeIndexShouldThrow()
        {
            var subject = CreateSubject();
            var testArray = CreateTestArray();

            subject.FetchBudgetBucket(testArray, -12, BucketRepositoryMock.Object);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FetchBudgetBucketWithNullArrayShouldThrow()
        {
            var subject = CreateSubject();

            subject.FetchBudgetBucket(null, 2, BucketRepositoryMock.Object);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FetchBudgetBucketWithNullBucketRepositoryShouldThrow()
        {
            var subject = CreateSubject();
            var testArray = CreateTestArray();

            subject.FetchBudgetBucket(testArray, 2, null);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedIndexException))]
        public void FetchBudgetBucketWithOutOfRangeIndexShouldThrow()
        {
            var subject = CreateSubject();
            var testArray = CreateTestArray();

            subject.FetchBudgetBucket(testArray, 12, BucketRepositoryMock.Object);

            Assert.Fail();
        }

        [TestMethod]
        public void FetchBudgetBucketWithValidParamsShouldReturnBucketObject()
        {
            var subject = CreateSubject();
            var testArray = CreateTestArray();
            var expectedResult = new SpentPerPeriodExpenseBucket("FUEL", "Fuel");
            BucketRepositoryMock.Setup(m => m.GetByCode("FUEL")).Returns(expectedResult);

            var result = subject.FetchBudgetBucket(testArray, 2, BucketRepositoryMock.Object);

            BucketRepositoryMock.Verify();
            Assert.AreSame(expectedResult, result);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void FetchDateWithInvalidDateShouldReturnMinDateTime()
        {
            var subject = CreateSubject();
            var testArray = CreateTestArray();

            subject.FetchDate(testArray, 3);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedIndexException))]
        public void FetchDateWithNegativeOutOfRangeIndexShouldThrow()
        {
            var subject = CreateSubject();
            var testArray = CreateTestArray();

            subject.FetchDate(testArray, -12);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FetchDateWithNullArrayShouldThrow()
        {
            var subject = CreateSubject();

            subject.FetchDate(null, 2);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedIndexException))]
        public void FetchDateWithOutOfRangeIndexShouldThrow()
        {
            var subject = CreateSubject();
            var testArray = CreateTestArray();

            subject.FetchDate(testArray, 12);

            Assert.Fail();
        }

        [TestMethod]
        public void FetchDateWithValidDateStringShouldReturnDate()
        {
            var subject = CreateSubject();
            var testArray = CreateTestArray();

            var result = subject.FetchDate(testArray, 1);

            Assert.IsInstanceOfType(result, typeof(DateTime));
            Assert.AreNotEqual(DateTime.MinValue, result);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void FetchDecimalWithInvalidDecimalShouldThrow()
        {
            var subject = CreateSubject();
            var testArray = CreateTestArray();

            subject.FetchDecimal(testArray, 2);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedIndexException))]
        public void FetchDecimalWithNegativeOutOfRangeIndexShouldThrow()
        {
            var subject = CreateSubject();
            var testArray = CreateTestArray();

            subject.FetchDecimal(testArray, -12);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FetchDecimalWithNullArrayShouldThrow()
        {
            var subject = CreateSubject();

            subject.FetchDecimal(null, 2);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedIndexException))]
        public void FetchDecimalWithOutOfRangeIndexShouldThrow()
        {
            var subject = CreateSubject();
            var testArray = CreateTestArray();

            subject.FetchDecimal(testArray, 12);

            Assert.Fail();
        }

        [TestMethod]
        public void FetchDecimalWithValidDecimalStringShouldReturnDecimal()
        {
            var subject = CreateSubject();
            var testArray = CreateTestArray();

            var result = subject.FetchDecimal(testArray, 0);

            Assert.IsInstanceOfType(result, typeof(decimal));
            Assert.AreNotEqual(decimal.MinValue, result);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void FetchGuidWithInvalidGuidShouldThrow()
        {
            var subject = CreateSubject();
            var testArray = CreateTestArray();

            subject.FetchGuid(testArray, 2);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedIndexException))]
        public void FetchGuidWithNegativeOutOfRangeIndexShouldThrow()
        {
            var subject = CreateSubject();
            var testArray = CreateTestArray();

            subject.FetchGuid(testArray, -12);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FetchGuidWithNullArrayShouldThrow()
        {
            var subject = CreateSubject();

            subject.FetchGuid(null, 2);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedIndexException))]
        public void FetchGuidWithOutOfRangeIndexShouldThrow()
        {
            var subject = CreateSubject();
            var testArray = CreateTestArray();

            subject.FetchGuid(testArray, 12);

            Assert.Fail();
        }

        [TestMethod]
        public void FetchGuidWithValidGuidStringShouldReturnGuid()
        {
            var subject = CreateSubject();
            var testArray = CreateTestArray();

            var result = subject.FetchGuid(testArray, 4);

            Assert.IsInstanceOfType(result, typeof(Guid));
            Assert.AreNotEqual(Guid.Empty, result);
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedIndexException))]
        public void FetchStringWithNegativeOutOfRangeIndexShouldThrow()
        {
            var subject = CreateSubject();
            var testArray = CreateTestArray();

            subject.FetchString(testArray, -12);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FetchStringWithNullArrayShouldThrow()
        {
            var subject = CreateSubject();

            subject.FetchString(null, 2);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedIndexException))]
        public void FetchStringWithOutOfRangeIndexShouldThrow()
        {
            var subject = CreateSubject();
            var testArray = CreateTestArray();

            subject.FetchString(testArray, 12);

            Assert.Fail();
        }

        [TestMethod]
        public void FetchStringWithValidStringShouldReturnString()
        {
            var subject = CreateSubject();
            var testArray = CreateTestArray();

            var result = subject.FetchString(testArray, 2);

            Assert.IsInstanceOfType(result, typeof(string));
            Assert.AreNotEqual(string.Empty, result);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void FetchStringShouldRemoveQuotes()
        {
            var subject = CreateSubject();
            var myData = new[] { "\"Test String\"", "no quotes", "-21.45" };
            Console.WriteLine($"Input:");
            Array.ForEach(myData, x => Console.Write($"{x}, "));
            var result1 = subject.FetchString(myData, 0);
            Console.WriteLine();
            Console.WriteLine($"{result1}");
            Assert.AreEqual(-1, result1.IndexOf('"'));
        }

        private BankImportUtilities CreateSubject()
        {
            BucketRepositoryMock = new Mock<IBudgetBucketRepository>();

            var subject = new BankImportUtilities(new FakeLogger());
            subject.ConfigureLocale(CultureInfo.CreateSpecificCulture("en-NZ"));
            return subject;
        }

        private string[] CreateTestArray()
        {
            return new[] { "123.34", "14/04/2014", "FUEL", "42/12/2088", "A94B4FE5-4F43-43A6-8CD2-8430F45FB58D" };
        }
    }
}
