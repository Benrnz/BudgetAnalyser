using System.Threading.Tasks;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.UnitTest.Services
{
    [TestClass]
    public class TransactionManagerServiceTest
    {
        private Mock<IBudgetBucketRepository> mockBudgetBucketRepo;
        private Mock<IStatementRepository> mockStatementRepo;
        private TransactionManagerService subject;

        [TestMethod]
        public void GivenTestData1AverageDebitShouldBe115()
        {
            Assert.AreEqual(-115.25M, decimal.Round(this.subject.AverageDebit, 2));
        }

        [TestMethod]
        public void GivenTestData1TotalCountShouldBe7()
        {
            Assert.AreEqual(10, this.subject.TotalCount);
        }

        [TestMethod]
        public void GivenTestData1TotalDebitsShouldBe806()
        {
            Assert.AreEqual(-806.78M, this.subject.TotalDebits);
        }

        [TestMethod]
        public void GivenTestData2TotalCreditsShouldBe2552()
        {
            Assert.AreEqual(2552.97M, this.subject.TotalCredits);
        }

        [TestMethod]
        public void GivenTestData2TotalDifferenceShouldBe1746()
        {
            Assert.AreEqual(1746.19M, this.subject.TotalCredits + this.subject.TotalDebits);
        }

        [TestMethod]
        public async Task LoadStatementModelAsync_ShouldReturnAStatementModel_GivenValidStorageKey()
        {
            this.mockStatementRepo.Setup(m => m.LoadStatementModelAsync(It.IsAny<string>())).Returns(Task.FromResult(new StatementModel(new FakeLogger())));

            var statement = await this.subject.LoadStatementModelAsync(@"C:\Foo.csv");

            Assert.IsNotNull(statement);
        }

        [TestInitialize]
        public void TestInit()
        {
            this.mockBudgetBucketRepo = new Mock<IBudgetBucketRepository>();
            this.mockStatementRepo = new Mock<IStatementRepository>();
            this.subject = new TransactionManagerService(this.mockBudgetBucketRepo.Object, this.mockStatementRepo.Object, new FakeLogger());
            this.mockStatementRepo
                .Setup(m => m.LoadStatementModelAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(StatementModelTestData.TestData2()));
            this.subject.LoadStatementModelAsync("Foo").Wait();
        }
    }
}