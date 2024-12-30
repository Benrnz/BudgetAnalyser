using System.Text;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Budget
{
    [TestClass]
    public class IncomeTest
    {
        public StringBuilder Logs { get; private set; }

        [TestMethod]
        public void BucketMustHaveADescription()
        {
            var subject = CreateSubject();
            subject.Bucket.Description = null;

            var result = subject.Validate(Logs);

            Assert.IsFalse(result);
            Assert.IsTrue(Logs.Length > 0);
        }

        [TestMethod]
        public void MaxDeciamlIsValidIncome()
        {
            var subject = CreateSubject();
            subject.Amount = decimal.MaxValue;

            var result = subject.Validate(Logs);

            Assert.IsTrue(result);
            Assert.IsTrue(Logs.Length == 0);
        }

        [TestMethod]
        public void MustBeAnIncomeBucket()
        {
            var subject = CreateSubject();
            subject.Bucket = new SavedUpForExpenseBucket("Foo", "Bar");

            var result = subject.Validate(Logs);

            Assert.IsFalse(result);
            Assert.IsTrue(Logs.Length > 0);
        }

        [TestMethod]
        public void NegativeAmountIsNotValid()
        {
            var subject = CreateSubject();
            subject.Amount = -5;

            var result = subject.Validate(Logs);

            Assert.IsFalse(result);
            Assert.IsTrue(Logs.Length > 0);
        }

        [TestMethod]
        public void NegativeAmountIsNotValidEvenWhenInActive()
        {
            var subject = CreateSubject();
            subject.Amount = -5;
            subject.Bucket.Active = false;

            var result = subject.Validate(Logs);

            Assert.IsFalse(result);
            Assert.IsTrue(Logs.Length > 0);
        }

        [TestMethod]
        public void OneCentIsValidIncome()
        {
            var subject = CreateSubject();

            var result = subject.Validate(Logs);

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
            var subject = CreateSubject();
            subject.Amount = 0;

            var result = subject.Validate(Logs);

            Assert.IsTrue(result);
        }

        private Income CreateSubject()
        {
            return new Income { Amount = 0.01M, Bucket = new IncomeBudgetBucket(TestDataConstants.IncomeBucketCode, "Foo Bar") };
        }
    }
}
