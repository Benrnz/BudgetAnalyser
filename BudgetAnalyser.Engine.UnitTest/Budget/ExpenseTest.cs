using System.Text;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Budget
{
    [TestClass]
    public class ExpenseTest
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
        public void MaxDeciamlIsValidAmount()
        {
            var subject = CreateSubject();
            subject.Amount = decimal.MaxValue;

            var result = subject.Validate(Logs);

            Assert.IsTrue(result);
            Assert.IsTrue(Logs.Length == 0);
        }

        [TestMethod]
        public void MustBeAnExpenseBucket()
        {
            var subject = CreateSubject();
            subject.Bucket = new IncomeBudgetBucket("Foo", "Bar");

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
        public void NegativeAmountIsNotValidEvenWhenInactive()
        {
            var subject = CreateSubject();
            subject.Amount = -5;
            subject.Bucket.Active = false;

            var result = subject.Validate(Logs);

            Assert.IsFalse(result);
            Assert.IsTrue(Logs.Length > 0);
        }

        [TestMethod]
        public void OneCentIsValidAmount()
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
        public void ZeroAmountIsNotValidWhenActive()
        {
            var subject = CreateSubject();
            subject.Amount = 0;
            subject.Bucket.Active = true;

            var result = subject.Validate(Logs);

            Assert.IsFalse(result);
            Assert.IsTrue(Logs.Length > 0);
        }

        [TestMethod]
        public void ZeroAmountIsValidWhenInActive()
        {
            var subject = CreateSubject();
            subject.Amount = 0;
            subject.Bucket.Active = false;

            var result = subject.Validate(Logs);

            Assert.IsTrue(result);
        }

        private Expense CreateSubject()
        {
            return new Expense { Amount = 0.01M, Bucket = new SpentPerPeriodExpenseBucket(TestDataConstants.CarMtcBucketCode, "Foo Bar") };
        }
    }
}
