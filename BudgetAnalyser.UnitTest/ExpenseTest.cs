using System.Text;
using BudgetAnalyser.Engine.Budget;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class ExpenseTest
    {
        [TestMethod]
        public void OneCentIsValidAmount()
        {
            var subject = CreateSubject();

            var result = subject.Validate(Logs);

            Assert.IsTrue(result);
            Assert.IsTrue(Logs.Length == 0);
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
        public void NegativeAmountIsNotValid()
        {
            var subject = CreateSubject();
            subject.Amount = -5;

            var result = subject.Validate(Logs);

            Assert.IsFalse(result);
            Assert.IsTrue(Logs.Length > 0);
        }

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
        public void MustBeAnExpenseBucket()
        {
            var subject = CreateSubject();
            subject.Bucket = new IncomeBudgetBucket("Foo", "Bar");

            var result = subject.Validate(Logs);

            Assert.IsFalse(result);
            Assert.IsTrue(Logs.Length > 0);
        }

        public StringBuilder Logs { get; private set; }

        [TestInitialize]
        public void TestInitialize()
        {
            Logs = new StringBuilder();
        }

        private Expense CreateSubject()
        {
            return new Expense { Amount = 0.01M, Bucket = new SpentMonthlyExpenseBucket(TestData.TestDataConstants.CarMtcBucketCode, "Foo Bar") };
        }
    }
}
