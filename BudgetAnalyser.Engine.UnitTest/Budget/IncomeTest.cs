using System.Text;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class IncomeTest
    {
        public StringBuilder Logs { get; private set; }

        [TestMethod]
        public void BucketMustHaveADescription()
        {
            Income subject = CreateSubject();
            subject.Bucket.Description = null;

            bool result = subject.Validate(Logs);

            Assert.IsFalse(result);
            Assert.IsTrue(Logs.Length > 0);
        }

        [TestMethod]
        public void MaxDeciamlIsValidIncome()
        {
            Income subject = CreateSubject();
            subject.Amount = decimal.MaxValue;

            bool result = subject.Validate(Logs);

            Assert.IsTrue(result);
            Assert.IsTrue(Logs.Length == 0);
        }

        [TestMethod]
        public void MustBeAnIncomeBucket()
        {
            Income subject = CreateSubject();
            subject.Bucket = new SavedUpForExpenseBucket("Foo", "Bar");

            bool result = subject.Validate(Logs);

            Assert.IsFalse(result);
            Assert.IsTrue(Logs.Length > 0);
        }

        [TestMethod]
        public void NegativeAmountIsNotValid()
        {
            Income subject = CreateSubject();
            subject.Amount = -5;

            bool result = subject.Validate(Logs);

            Assert.IsFalse(result);
            Assert.IsTrue(Logs.Length > 0);
        }

        [TestMethod]
        public void NegativeAmountIsNotValidEvenWhenInActive()
        {
            Income subject = CreateSubject();
            subject.Amount = -5;
            subject.Bucket.Active = false;

            bool result = subject.Validate(Logs);

            Assert.IsFalse(result);
            Assert.IsTrue(Logs.Length > 0);
        }

        [TestMethod]
        public void OneCentIsValidIncome()
        {
            Income subject = CreateSubject();

            bool result = subject.Validate(Logs);

            Assert.IsTrue(result);
            Assert.IsTrue(Logs.Length == 0);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            Logs = new StringBuilder();
        }

        [TestMethod]
        public void ZeroAmountIsValid()
        {
            Income subject = CreateSubject();
            subject.Amount = 0;

            bool result = subject.Validate(Logs);

            Assert.IsTrue(result);
        }

        private Income CreateSubject()
        {
            return new Income { Amount = 0.01M, Bucket = new IncomeBudgetBucket(TestDataConstants.IncomeBucketCode, "Foo Bar") };
        }
    }
}