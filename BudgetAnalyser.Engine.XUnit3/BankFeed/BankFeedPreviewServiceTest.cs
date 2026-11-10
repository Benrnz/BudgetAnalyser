using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.BankFeed;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.BankFeed;

public class BankFeedPreviewServiceTest
{
    // Wednesday 11 November 2026, so the last business day is Tuesday 10 November.
    private static readonly DateOnly Today = new(2026, 11, 11);
    private static readonly ImportDateRange ExpectedRange = new(new DateOnly(2026, 10, 12), new DateOnly(2026, 11, 10));

    private readonly BankFeedAccountMapping mapping = new("acc_test_1", new ChequeAccount(AccountTypeRepositoryConstants.Cheque), BankFeedFieldMapping.Default);
    private readonly IBankFeedProvider provider = Substitute.For<IBankFeedProvider>();
    private readonly BankFeedPreviewService subject;

    public BankFeedPreviewServiceTest()
    {
        this.provider.ProviderKey.Returns("test.provider");
        var importer = new BankFeedTransactionsImporter(this.provider, Substitute.For<IBankFeedConfigurationService>(), new InMemoryTransactionTypeRepository(), new FakeLogger());
        this.subject = new BankFeedPreviewService(importer);
    }

    [Fact]
    public async Task PreviewAsync_ShouldRequestThirtyDaysUpToLastBusinessDay()
    {
        this.provider.GetTransactionsAsync("acc_test_1", Arg.Any<ImportDateRange>(), Arg.Any<CancellationToken>()).Returns([]);

        await this.subject.PreviewAsync(this.mapping, Today, 10, CancellationToken.None);

        await this.provider.Received(1).GetTransactionsAsync("acc_test_1", ExpectedRange, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PreviewAsync_ShouldReturnNewestTransactionsFirst_LimitedToCount()
    {
        var feed = Enumerable.Range(1, 15)
            .Select(day => new BankFeedTransaction($"trans_test_{day}", "acc_test_1", "Test Bank A", new DateOnly(2026, 10, 12).AddDays(day), $"TEST {day}", -day, null, null, null, null, null, null, null))
            .ToList();
        this.provider.GetTransactionsAsync("acc_test_1", ExpectedRange, Arg.Any<CancellationToken>()).Returns(feed);

        var result = await this.subject.PreviewAsync(this.mapping, Today, 10, CancellationToken.None);

        result.Count.ShouldBe(10);
        result[0].Description.ShouldBe("TEST 15");
        result[9].Description.ShouldBe("TEST 6");
    }
}
