using System;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class BankImportUtilitiesTest
    {
        private Mock<IBudgetBucketRepository> BucketRepositoryMock { get; set; }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedIndexException))]
        public void FetchBudgetBucketWithNegativeOutOfRangeIndexShouldThrow()
        {
            BankImportUtilities subject = CreateSubject();
            string[] testArray = CreateTestArray();

            subject.FetchBudgetBucket(testArray, -12, BucketRepositoryMock.Object);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void FetchBudgetBucketWithNullArrayShouldThrow()
        {
            BankImportUtilities subject = CreateSubject();

            subject.FetchBudgetBucket(null, 2, BucketRepositoryMock.Object);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void FetchBudgetBucketWithNullBucketRepositoryShouldThrow()
        {
            BankImportUtilities subject = CreateSubject();
            string[] testArray = CreateTestArray();

            subject.FetchBudgetBucket(testArray, 2, null);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof (UnexpectedIndexException))]
        public void FetchBudgetBucketWithOutOfRangeIndexShouldThrow()
        {
            BankImportUtilities subject = CreateSubject();
            string[] testArray = CreateTestArray();

            subject.FetchBudgetBucket(testArray, 12, BucketRepositoryMock.Object);

            Assert.Fail();
        }

        [TestMethod]
        public void FetchBudgetBucketWithValidParamsShouldReturnBucketObject()
        {
            BankImportUtilities subject = CreateSubject();
            string[] testArray = CreateTestArray();
            var expectedResult = new SpentMonthlyExpenseBucket("FUEL", "Fuel");
            BucketRepositoryMock.Setup(m => m.GetByCode("FUEL")).Returns(expectedResult);

            BudgetBucket result = subject.FetchBudgetBucket(testArray, 2, BucketRepositoryMock.Object);

            BucketRepositoryMock.Verify();
            Assert.AreSame(expectedResult, result);
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedIndexException))]
        public void SafeArrayFetchDateWithNegativeOutOfRangeIndexShouldThrow()
        {
            BankImportUtilities subject = CreateSubject();
            string[] testArray = CreateTestArray();

            subject.SafeArrayFetchDate(testArray, -12);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void SafeArrayFetchDateWithNullArrayShouldThrow()
        {
            BankImportUtilities subject = CreateSubject();

            subject.SafeArrayFetchDate(null, 2);

            Assert.Fail();
        }

        [TestMethod]
        public void SafeArrayFetchDateWithValidDateStringShouldReturnDate()
        {
            BankImportUtilities subject = CreateSubject();
            string[] testArray = CreateTestArray();

            DateTime result = subject.SafeArrayFetchDate(testArray, 1);

            Assert.IsInstanceOfType(result, typeof (DateTime));
            Assert.AreNotEqual(DateTime.MinValue, result);
        }

        [TestMethod]
        public void SafeArrayFetchDatetWithInvalidDateShouldReturnMinDateTime()
        {
            BankImportUtilities subject = CreateSubject();
            string[] testArray = CreateTestArray();

            DateTime result = subject.SafeArrayFetchDate(testArray, 3);

            Assert.AreEqual(DateTime.MinValue, result);
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedIndexException))]
        public void SafeArrayFetchDatetWithOutOfRangeIndexShouldThrow()
        {
            BankImportUtilities subject = CreateSubject();
            string[] testArray = CreateTestArray();

            subject.SafeArrayFetchDate(testArray, 12);

            Assert.Fail();
        }


        [TestMethod]
        [ExpectedException(typeof(UnexpectedIndexException))]
        public void SafeArrayFetchDecimalWithNegativeOutOfRangeIndexShouldThrow()
        {
            BankImportUtilities subject = CreateSubject();
            string[] testArray = CreateTestArray();

            subject.SafeArrayFetchDecimal(testArray, -12);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void SafeArrayFetchDecimalWithNullArrayShouldThrow()
        {
            BankImportUtilities subject = CreateSubject();

            subject.SafeArrayFetchDecimal(null, 2);

            Assert.Fail();
        }

        [TestMethod]
        public void SafeArrayFetchDecimalWithValidDecimalStringShouldReturnDecimal()
        {
            BankImportUtilities subject = CreateSubject();
            string[] testArray = CreateTestArray();

            Decimal result = subject.SafeArrayFetchDecimal(testArray, 0);

            Assert.IsInstanceOfType(result, typeof (Decimal));
            Assert.AreNotEqual(Decimal.MinValue, result);
        }

        [TestMethod]
        public void SafeArrayFetchDecimaltWithInvalidDecimalShouldReturnMinDecimal()
        {
            BankImportUtilities subject = CreateSubject();
            string[] testArray = CreateTestArray();

            decimal result = subject.SafeArrayFetchDecimal(testArray, 2);

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedIndexException))]
        public void SafeArrayFetchDecimaltWithOutOfRangeIndexShouldThrow()
        {
            BankImportUtilities subject = CreateSubject();
            string[] testArray = CreateTestArray();

            subject.SafeArrayFetchDecimal(testArray, 12);

            Assert.Fail();
        }


        [TestMethod]
        [ExpectedException(typeof(UnexpectedIndexException))]
        public void SafeArrayFetchGuidWithNegativeOutOfRangeIndexShouldThrow()
        {
            BankImportUtilities subject = CreateSubject();
            string[] testArray = CreateTestArray();

            subject.SafeArrayFetchGuid(testArray, -12);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void SafeArrayFetchGuidWithNullArrayShouldThrow()
        {
            BankImportUtilities subject = CreateSubject();

            subject.SafeArrayFetchGuid(null, 2);

            Assert.Fail();
        }

        [TestMethod]
        public void SafeArrayFetchGuidWithValidGuidStringShouldReturnGuid()
        {
            BankImportUtilities subject = CreateSubject();
            string[] testArray = CreateTestArray();

            Guid result = subject.SafeArrayFetchGuid(testArray, 0);

            Assert.IsInstanceOfType(result, typeof (Guid));
            Assert.AreNotEqual(Guid.Empty, result);
        }

        [TestMethod]
        public void SafeArrayFetchGuidtWithInvalidGuidShouldReturnNewGuid()
        {
            BankImportUtilities subject = CreateSubject();
            string[] testArray = CreateTestArray();

            Guid result = subject.SafeArrayFetchGuid(testArray, 2);

            Assert.AreNotEqual(Guid.Empty, result);
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedIndexException))]
        public void SafeArrayFetchGuidtWithOutOfRangeIndexShouldThrow()
        {
            BankImportUtilities subject = CreateSubject();
            string[] testArray = CreateTestArray();

            subject.SafeArrayFetchGuid(testArray, 12);

            Assert.Fail();
        }


        [TestMethod]
        [ExpectedException(typeof(UnexpectedIndexException))]
        public void SafeArrayFetchStringWithNegativeOutOfRangeIndexShouldThrow()
        {
            BankImportUtilities subject = CreateSubject();
            string[] testArray = CreateTestArray();

            subject.SafeArrayFetchString(testArray, -12);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void SafeArrayFetchStringWithNullArrayShouldThrow()
        {
            BankImportUtilities subject = CreateSubject();

            subject.SafeArrayFetchString(null, 2);

            Assert.Fail();
        }

        [TestMethod]
        public void SafeArrayFetchStringWithValidStringShouldReturnString()
        {
            BankImportUtilities subject = CreateSubject();
            string[] testArray = CreateTestArray();

            String result = subject.SafeArrayFetchString(testArray, 2);

            Assert.IsInstanceOfType(result, typeof (String));
            Assert.AreNotEqual(String.Empty, result);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedIndexException))]
        public void SafeArrayFetchStringtWithOutOfRangeIndexShouldThrow()
        {
            BankImportUtilities subject = CreateSubject();
            string[] testArray = CreateTestArray();

            subject.SafeArrayFetchString(testArray, 12);

            Assert.Fail();
        }

        private BankImportUtilities CreateSubject()
        {
            BucketRepositoryMock = new Mock<IBudgetBucketRepository>();

            return new BankImportUtilities(new FakeLogger());
        }

        private string[] CreateTestArray()
        {
            return new[] { "123.34", new DateTime(2014, 04, 14).ToShortDateString(), "FUEL", "42/12/2088" };
        }
    }
}