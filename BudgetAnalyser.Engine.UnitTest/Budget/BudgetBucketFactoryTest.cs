using System;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Budget
{
    [TestClass]
    public class BudgetBucketFactoryTest
    {
        private BudgetBucketFactory Subject { get; set; }

        [TestMethod]
        public void BucketDtoTypeEnumHas6SupportedValues()
        {
            // If this test fails you may need to consider adding code to the BudgetBucketFactory class.
            Assert.AreEqual(7, Enum.GetNames(typeof(BucketDtoType)).Length);
        }

        [TestMethod]
        public void BuildShouldCreateFixedBudgetProject()
        {
            Assert.IsInstanceOfType(
                Subject.BuildModel(
                    new FixedBudgetBucketDto
                    {
                        Type = BucketDtoType.FixedBudgetProject,
                        Created = DateTime.Now,
                        Code = "SURPLUS.FOO",
                        Description = "Fu bar"
                    }),
                typeof(FixedBudgetProjectBucket));
        }

        [TestMethod]
        public void BuildShouldCreateIncomeBucket()
        {
            Assert.IsInstanceOfType(
                Subject.BuildModel(
                    new BudgetBucketDto
                    {
                        Type = BucketDtoType.Income
                    }),
                typeof(IncomeBudgetBucket));
        }

        [TestMethod]
        public void BuildShouldCreateSavedUpForExpenseBucket()
        {
            Assert.IsInstanceOfType(
                Subject.BuildModel(
                    new BudgetBucketDto {Type = BucketDtoType.SavedUpForExpense}),
                typeof(SavedUpForExpenseBucket));
        }

        [TestMethod]
        public void BuildShouldCreateSavingsCommittmentBucket()
        {
            Assert.IsInstanceOfType(
                Subject.BuildModel(
                    new BudgetBucketDto { Type = BucketDtoType.SavingsCommitment }), 
                typeof(SavingsCommitmentBucket));
        }

        [TestMethod]
        public void BuildShouldCreateSpentMonthlyBucket()
        {
            Assert.IsInstanceOfType(
                Subject.BuildModel(new BudgetBucketDto { Type = BucketDtoType.SpentMonthlyExpense }), 
                typeof(SpentPerPeriodExpenseBucket));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void BuildShouldThrowGivenPayCreditCard()
        {
            Subject.BuildModel(new BudgetBucketDto { Type = BucketDtoType.PayCreditCard });
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void BuildShouldThrowGivenSurplus()
        {
            Subject.BuildModel(new BudgetBucketDto { Type = BucketDtoType.Surplus});
            Assert.Fail();
        }

        [TestMethod]
        public void SerialiseTypeShouldReturnIncomeGivenIncomeBucket()
        {
            Assert.AreEqual(BucketDtoType.Income, Subject.SerialiseType(new IncomeBudgetBucket()));
        }

        [TestMethod]
        public void SerialiseTypeShouldReturnPayCreditCardGivenPayCreditCardBucket()
        {
            Assert.AreEqual(BucketDtoType.PayCreditCard, Subject.SerialiseType(new PayCreditCardBucket()));
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
            Assert.AreEqual(BucketDtoType.SpentMonthlyExpense, Subject.SerialiseType(new SpentPerPeriodExpenseBucket()));
        }

        [TestMethod]
        public void SerialiseTypeShouldReturnSurplusGivenSurplusBucket()
        {
            Assert.AreEqual(BucketDtoType.Surplus, Subject.SerialiseType(new SurplusBucket()));
        }

        [TestMethod]
        public void SerialiseTypeShouldThrowGivenFixedBudgetProject()
        {
            Assert.AreEqual(BucketDtoType.FixedBudgetProject, Subject.SerialiseType(new FixedBudgetProjectBucket("Foo", "Foobar", 1000)));
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