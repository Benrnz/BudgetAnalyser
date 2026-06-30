using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Transactions;
using BudgetAnalyser.Wpf.XUnit3.TestData;
using NSubstitute;
using Shouldly;

namespace BudgetAnalyser.Wpf.XUnit3.Transactions;

public class TransactionsListViewModelTest
{
    private readonly ITransactionManagerService mockTransactionService;
    private IUiContext mockUiContext;

    public TransactionsListViewModelTest()
    {
        this.mockUiContext = Substitute.For<IUiContext>();
        this.mockTransactionService = Substitute.For<ITransactionManagerService>();
        this.mockTransactionService.DetectDuplicateTransactions().Returns(string.Empty);
    }

    [Fact]
    public void GivenNoDataHasTransactionsShouldBeFalse()
    {
        var subject = CreateSubject();
        subject.HasTransactions.ShouldBeFalse();
    }

    [Fact]
    public void GivenNoDataTransactionListModelNameShouldBeNoTransactionsLoaded()
    {
        var subject = CreateSubject();
        subject.TransactionListModelName.ShouldBe("[No Transactions Loaded]");
    }

    [Fact]
    public void GivenTestData1HasTransactionsShouldBeTrue()
    {
        var subject = Arrange();
        subject.HasTransactions.ShouldBeTrue();
    }

    [Fact]
    public void GivenTestData1TotalCreditsShouldBe0()
    {
        var subject = Arrange();
        subject.TotalCredits.ShouldBe(0);
    }

    [Fact]
    public void GivenTestData1TransactionListModelNameShouldBeFooBar()
    {
        var subject = Arrange();
        subject.TransactionListModelName.ShouldBe("FooTransactions");
    }

    [Fact]
    public void TriggerRefreshTotalsRowShouldRaise7Events()
    {
        var subject = Arrange();
        var eventCount = 0;
        subject.PropertyChanged += (_, _) => eventCount++;
        subject.TriggerRefreshTotalsRow();

        eventCount.ShouldBe(7);
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
}
