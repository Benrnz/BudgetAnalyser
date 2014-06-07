using System.Text;
using BudgetAnalyser.Engine.Budget;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class IncomeTest
    {
        [TestMethod]
        public void OneCentIsValidIncome()
        {
            var subject = CreateSubject();

            var result = subject.Validate(this.Logs);

            Assert.IsTrue(result);
            Assert.IsTrue(this.Logs.Length == 0);
        }

        [TestMethod]
        public void MaxDeciamlIsValidIncome()
        {
            var subject = CreateSubject();
            subject.Amount = decimal.MaxValue;

            var result = subject.Validate(this.Logs);

            Assert.IsTrue(result);
            Assert.IsTrue(this.Logs.Length == 0);
        }

        [TestMethod]
        public void NegativeAmountIsNotValid()
        {
            var subject = CreateSubject();
            subject.Amount = -5;

            var result = subject.Validate(this.Logs);

            Assert.IsFalse(result);
            Assert.IsTrue(this.Logs.Length > 0);
        }

        [TestMethod]
        public void BucketMustHaveADescription()
        {
            var subject = CreateSubject();
            subject.Bucket.Description = null;

            var result = subject.Validate(this.Logs);

            Assert.IsFalse(result);
            Assert.IsTrue(this.Logs.Length > 0);
        }

        [TestMethod]
        public void MustBeAnIncomeBucket()
        {
            var subject = CreateSubject();
            subject.Bucket = new SavedUpForExpenseBucket("Foo", "Bar");

            var result = subject.Validate(this.Logs);

            Assert.IsFalse(result);
            Assert.IsTrue(this.Logs.Length > 0);
        }

        public StringBuilder Logs { get; private set; }

        [TestInitialize]
        public void TestInitialize()
        {
            this.Logs = new StringBuilder();
        }

        private Income CreateSubject()
        {
            return new Income { Amount = 0.01M, Bucket = new IncomeBudgetBucket(TestData.TestDataConstants.IncomeBucketCode, "Foo Bar") };
        }
    }
}
