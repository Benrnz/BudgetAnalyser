using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Transactions;
using BudgetAnalyser.Wpf.UnitTest.TestData;
using NSubstitute;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.Wpf.UnitTest.Transactions;

[TestClass]
public class TransactionsListViewModelTest
{
    private ITransactionManagerService mockTransactionService;
    private IUiContext mockUiContext;

    [TestMethod]
    public void GivenNoDataHasTransactionsShouldBeFalse()
    {
        var subject = CreateSubject();
        Assert.IsFalse(subject.HasTransactions);
    }

    [TestMethod]
    public void GivenNoDataTransactionListModelNameShouldBeNoTransactionsLoaded()
    {
        var subject = CreateSubject();
        Assert.AreEqual("[No Transactions Loaded]", subject.TransactionListModelName);
    }

    [TestMethod]
    public void GivenTestData1HasTransactionsShouldBeTrue()
    {
        var subject = Arrange();
        Assert.IsTrue(subject.HasTransactions);
    }

    [TestMethod]
    public void GivenTestData1TransactionListModelNameShouldBeFooBar()
    {
        var subject = Arrange();
        Assert.AreEqual("FooStatement", subject.TransactionListModelName);
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
        this.mockUiContext = Substitute.For<IUiContext>();
        this.mockTransactionService = Substitute.For<ITransactionManagerService>();
        this.mockTransactionService.DetectDuplicateTransactions().Returns(string.Empty);
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

    private TransactionsListViewModel Arrange()
    {
        var subject = CreateSubject();
        subject.TransactionsList = TransactionsListModelTestData.TestData1();
        return subject;
    }

    private TransactionsListViewModel CreateSubject()
    {
        return new TransactionsListViewModel(Substitute.For<IApplicationDatabaseFacade>(), this.mockTransactionService);
    }

    private static IWaitCursor WaitCursorFactory()
    {
        return Substitute.For<IWaitCursor>();
    }
}
