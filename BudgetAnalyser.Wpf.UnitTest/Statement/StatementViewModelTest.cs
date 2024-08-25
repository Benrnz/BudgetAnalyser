using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Statement;
using BudgetAnalyser.Wpf.UnitTest.TestData;
using Moq;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.Wpf.UnitTest.Statement
{
    [TestClass]
    public class StatementViewModelTest
    {
        private Mock<ITransactionManagerService> mockTransactionService;
        private Mock<IUiContext> mockUiContext;

        [TestMethod]
        public void GivenNoDataHasTransactionsShouldBeFalse()
        {
            StatementViewModel subject = CreateSubject();
            Assert.IsFalse(subject.HasTransactions);
        }

        [TestMethod]
        public void GivenNoDataStatementNameShouldBeNoTransactionsLoaded()
        {
            StatementViewModel subject = CreateSubject();
            Assert.AreEqual("[No Transactions Loaded]", subject.StatementName);
        }

        [TestMethod]
        public void GivenTestData1HasTransactionsShouldBeTrue()
        {
            StatementViewModel subject = Arrange();
            Assert.IsTrue(subject.HasTransactions);
        }

        [TestMethod]
        public void GivenTestData1StatementNameShouldBeFooStatement()
        {
            StatementViewModel subject = Arrange();
            Assert.AreEqual("FooStatement", subject.StatementName);
        }

        [TestMethod]
        public void GivenTestData1TotalCreditsShouldBe0()
        {
            StatementViewModel subject = Arrange();
            Assert.AreEqual(0, subject.TotalCredits);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SetStatement_ShouldThrow_GivenInitialiseHasNotBeenCalled()
        {
            StatementViewModel subject = CreateSubject();
            subject.Statement = StatementModelTestData.TestData1();
            Assert.Fail();
        }

        [TestInitialize]
        public void TestInitialise()
        {
            this.mockUiContext = new Mock<IUiContext>();
            this.mockTransactionService = new Mock<ITransactionManagerService>();
            this.mockTransactionService.Setup(m => m.DetectDuplicateTransactions()).Returns(string.Empty);
        }

        [TestMethod]
        public void TriggerRefreshTotalsRowShouldRaise10Events()
        {
            StatementViewModel subject = Arrange();
            var eventCount = 0;
            subject.PropertyChanged += (s, e) => eventCount++;
            subject.TriggerRefreshTotalsRow();

            Assert.AreEqual(8, eventCount);
        }

        private static Transaction GetPhoneTxnFromFullList(StatementViewModel subject)
        {
            Transaction transactionFromFullList = subject.Statement.Transactions
                .Single(t => t.BudgetBucket == StatementModelTestData.PhoneBucket && t.Date == new DateTime(2013, 07, 16));
            return transactionFromFullList;
        }

        private static IWaitCursor WaitCursorFactory()
        {
            return new Mock<IWaitCursor>().Object;
        }

        private StatementViewModel Arrange()
        {
            StatementViewModel subject = CreateSubject().Initialise(this.mockTransactionService.Object);
            subject.Statement = StatementModelTestData.TestData1();
            return subject;
        }

        private StatementViewModel CreateSubject()
        {
            return new StatementViewModel(this.mockUiContext.Object, new Mock<IApplicationDatabaseFacade>().Object);
        }
    }
}