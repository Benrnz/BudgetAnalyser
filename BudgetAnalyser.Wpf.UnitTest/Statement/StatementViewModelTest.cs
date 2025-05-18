using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Statement;
using BudgetAnalyser.Wpf.UnitTest.TestData;
using Moq;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.Wpf.UnitTest.Statement;

[TestClass]
public class StatementViewModelTest
{
    private Mock<ITransactionManagerService> mockTransactionService;
    private Mock<IUiContext> mockUiContext;

    [TestMethod]
    public void GivenNoDataHasTransactionsShouldBeFalse()
    {
        var subject = CreateSubject();
        Assert.IsFalse(subject.HasTransactions);
    }

    [TestMethod]
    public void GivenNoDataStatementNameShouldBeNoTransactionsLoaded()
    {
        var subject = CreateSubject();
        Assert.AreEqual("[No Transactions Loaded]", subject.StatementName);
    }

    [TestMethod]
    public void GivenTestData1HasTransactionsShouldBeTrue()
    {
        var subject = Arrange();
        Assert.IsTrue(subject.HasTransactions);
    }

    [TestMethod]
    public void GivenTestData1StatementNameShouldBeFooStatement()
    {
        var subject = Arrange();
        Assert.AreEqual("FooStatement", subject.StatementName);
    }

    [TestMethod]
    public void GivenTestData1TotalCreditsShouldBe0()
    {
        var subject = Arrange();
        Assert.AreEqual(0, subject.TotalCredits);
    }

    [TestInitialize]
    public void TestInitialise()
    {
        this.mockUiContext = new Mock<IUiContext>();
        this.mockTransactionService = new Mock<ITransactionManagerService>();
        this.mockTransactionService.Setup(m => m.DetectDuplicateTransactions()).Returns(string.Empty);
    }

    [TestMethod]
    public void TriggerRefreshTotalsRowShouldRaise7Events()
    {
        var subject = Arrange();
        var eventCount = 0;
        subject.PropertyChanged += (s, e) => eventCount++;
        subject.TriggerRefreshTotalsRow();

        Assert.AreEqual(7, eventCount);
    }

    private StatementViewModel Arrange()
    {
        var subject = CreateSubject();
        subject.Statement = StatementModelTestData.TestData1();
        return subject;
    }

    private StatementViewModel CreateSubject()
    {
        return new StatementViewModel(new Mock<IApplicationDatabaseFacade>().Object, this.mockTransactionService.Object);
    }

    private static IWaitCursor WaitCursorFactory()
    {
        return new Mock<IWaitCursor>().Object;
    }
}
