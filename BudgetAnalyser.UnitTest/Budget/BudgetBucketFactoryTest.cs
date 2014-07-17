using System;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class BudgetBucketFactoryTest
    {
        private BudgetBucketFactory Subject { get; set; }

        [TestMethod]
        public void BucketDtoTypeEnumHas6SupportedValues()
        {
            // If this test fails you may need to consider adding code to the BudgetBucketFactory class.
            Assert.AreEqual(6, Enum.GetNames(typeof(BucketDtoType)).Length);
        }

        [TestMethod]
        public void BuildShouldCreateIncomeBucket()
        {
            Assert.IsInstanceOfType(Subject.Build(BucketDtoType.Income), typeof(IncomeBudgetBucket));
        }

        [TestMethod]
        public void BuildShouldCreateSavedUpForExpenseBucket()
        {
            Assert.IsInstanceOfType(Subject.Build(BucketDtoType.SavedUpForExpense), typeof(SavedUpForExpenseBucket));
        }

        [TestMethod]
        public void BuildShouldCreateSavingsCommittmentBucket()
        {
            Assert.IsInstanceOfType(Subject.Build(BucketDtoType.SavingsCommitment), typeof(SavingsCommitmentBucket));
        }

        [TestMethod]
        public void BuildShouldCreateSpentMonthlyBucket()
        {
            Assert.IsInstanceOfType(Subject.Build(BucketDtoType.SpentMonthlyExpense), typeof(SpentMonthlyExpenseBucket));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void BuildShouldThrowGivenJournal()
        {
            Subject.Build(BucketDtoType.Journal);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void BuildShouldThrowGivenSurplus()
        {
            Subject.Build(BucketDtoType.Surplus);
            Assert.Fail();
        }

        [TestMethod]
        public void SerialiseTypeShouldReturnIncomeGivenIncomeBucket()
        {
            Assert.AreEqual(BucketDtoType.Income, Subject.SerialiseType(new IncomeBudgetBucket()));
        }

        [TestMethod]
        public void SerialiseTypeShouldReturnJournalGivenJournalBucket()
        {
            Assert.AreEqual(BucketDtoType.Journal, Subject.SerialiseType(new JournalBucket()));
        }

        [TestMethod]
        public void SerialiseTypeShouldReturnSavedUpForGivenSavedUpForBucket()
        {
            Assert.AreEqual(BucketDtoType.SavedUpForExpense, Subject.SerialiseType(new SavedUpForExpenseBucket()));
        }

        [TestMethod]
        public void SerialiseTypeShouldReturnSavingsCommittmentGivenSavingsCommittmentBucket()
        {
            Assert.AreEqual(BucketDtoType.SavingsCommitment, Subject.SerialiseType(new SavingsCommitmentBucket()));
        }

        [TestMethod]
        public void SerialiseTypeShouldReturnSpentMonthlyGivenSpentMonthlyBucket()
        {
            Assert.AreEqual(BucketDtoType.SpentMonthlyExpense, Subject.SerialiseType(new SpentMonthlyExpenseBucket()));
        }

        [TestMethod]
        public void SerialiseTypeShouldReturnSurplusGivenSurplusBucket()
        {
            Assert.AreEqual(BucketDtoType.Surplus, Subject.SerialiseType(new SurplusBucket()));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SerialiseTypeShouldThrowGivenNullType()
        {
            Subject.SerialiseType(null);
            Assert.Fail();
        }

        [TestInitialize]
        public void TestInitialise()
        {
            Subject = Arrange();
        }

        private static BudgetBucketFactory Arrange()
        {
            return new BudgetBucketFactory();
        }
    }
}