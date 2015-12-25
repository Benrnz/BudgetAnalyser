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
            Expense subject = CreateSubject();
            subject.Bucket.Description = null;

            bool result = subject.Validate(Logs);

            Assert.IsFalse(result);
            Assert.IsTrue(Logs.Length > 0);
        }

        [TestMethod]
        public void MaxDeciamlIsValidAmount()
        {
            Expense subject = CreateSubject();
            subject.Amount = decimal.MaxValue;

            bool result = subject.Validate(Logs);

            Assert.IsTrue(result);
            Assert.IsTrue(Logs.Length == 0);
        }

        [TestMethod]
        public void MustBeAnExpenseBucket()
        {
            Expense subject = CreateSubject();
            subject.Bucket = new IncomeBudgetBucket("Foo", "Bar");

            bool result = subject.Validate(Logs);

            Assert.IsFalse(result);
            Assert.IsTrue(Logs.Length > 0);
        }

        [TestMethod]
        public void NegativeAmountIsNotValid()
        {
            Expense subject = CreateSubject();
            subject.Amount = -5;

            bool result = subject.Validate(Logs);

            Assert.IsFalse(result);
            Assert.IsTrue(Logs.Length > 0);
        }

        [TestMethod]
        public void NegativeAmountIsNotValidEvenWhenInactive()
        {
            Expense subject = CreateSubject();
            subject.Amount = -5;
            subject.Bucket.Active = false;

            bool result = subject.Validate(Logs);

            Assert.IsFalse(result);
            Assert.IsTrue(Logs.Length > 0);
        }

        [TestMethod]
        public void OneCentIsValidAmount()
        {
            Expense subject = CreateSubject();

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
        public void ZeroAmountIsNotValidWhenActive()
        {
            Expense subject = CreateSubject();
            subject.Amount = 0;
            subject.Bucket.Active = true;

            bool result = subject.Validate(Logs);

            Assert.IsFalse(result);
            Assert.IsTrue(Logs.Length > 0);
        }

        [TestMethod]
        public void ZeroAmountIsValidWhenInActive()
        {
            Expense subject = CreateSubject();
            subject.Amount = 0;
            subject.Bucket.Active = false;

            bool result = subject.Validate(Logs);

            Assert.IsTrue(result);
        }

        private Expense CreateSubject()
        {
            return new Expense { Amount = 0.01M, Bucket = new SpentMonthlyExpenseBucket(TestDataConstants.CarMtcBucketCode, "Foo Bar") };
        }
    }
}