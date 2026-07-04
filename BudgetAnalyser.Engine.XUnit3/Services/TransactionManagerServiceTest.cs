using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Engine.XUnit.Helpers;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using Rees.UnitTestUtilities;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Services;

public class TransactionManagerServiceTest
{
    private readonly IBudgetBucketRepository mockBudgetBucketRepo;
    private readonly ITransactionsListModelRepository mockTransactionsRepo;
    private readonly ApplicationDatabase testAppDb = new();
    private IBudgetBucketRepository budgetBucketRepo;
    private TransactionManagerService subject = null!;
    private TransactionsListModel testData;

    public TransactionManagerServiceTest()
    {
        PrivateAccessor.SetProperty(this.testAppDb, "TransactionsListModelStorageKey", @"Foo.csv");
        PrivateAccessor.SetProperty(this.testAppDb, "FileName", @"C:\AppDb.bax");
        this.testData = TransactionsListModelTestData.TestData2();

        this.mockBudgetBucketRepo = Substitute.For<IBudgetBucketRepository>();
        this.mockTransactionsRepo = Substitute.For<ITransactionsListModelRepository>();

        this.mockTransactionsRepo.LoadAsync(Arg.Any<string>(), Arg.Any<bool>())
            .Returns(Task.FromResult(this.testData));

        this.mockBudgetBucketRepo.Buckets.Returns(BudgetBucketTestData.BudgetModelTestData1Buckets);
        this.budgetBucketRepo = this.mockBudgetBucketRepo;
        Arrange();
    }

    [Fact]
    public void Ctor_ShouldThrow_GivenNullBucketRepo()
    {
        Should.Throw<ArgumentNullException>(() =>
            new TransactionManagerService(null!, this.mockTransactionsRepo, new FakeLogger(), new FakeMonitorableDependencies()));
    }

    [Fact]
    public void Ctor_ShouldThrow_GivenNullLogger()
    {
        Should.Throw<ArgumentNullException>(() =>
            new TransactionManagerService(this.mockBudgetBucketRepo, this.mockTransactionsRepo, null!, new FakeMonitorableDependencies()));
    }

    [Fact]
    public void Ctor_ShouldThrow_GivenNullTransactionsRepo()
    {
        Should.Throw<ArgumentNullException>(() =>
            new TransactionManagerService(this.mockBudgetBucketRepo, null!, new FakeLogger(), new FakeMonitorableDependencies()));
    }

    [Fact]
    public void DetectDuplicateTransactions_ShouldReturnEmpty_GivenNullTransactionsModel()
    {
        this.subject = CreateSubject();
        this.subject.DetectDuplicateTransactions().ShouldBe(string.Empty);
    }

    [Fact]
    public void DetectDuplicateTransactions_ShouldReturnEmpty_GivenTestData2()
    {
        string.IsNullOrWhiteSpace(this.subject.DetectDuplicateTransactions()).ShouldBeTrue();
    }

    [Fact]
    public void DetectDuplicateTransactions_ShouldSummaryText_GivenTestData4()
    {
        this.testData = TransactionsListModelTestData.TestData4();
        Arrange();

        var result = this.subject.DetectDuplicateTransactions();
        Console.WriteLine(result);
        string.IsNullOrWhiteSpace(result).ShouldBeFalse();
    }

    [Fact]
    public void FilterableBuckets_ShouldContainABlankElement_GivenAnyBucketList()
    {
        this.subject.FilterableBuckets().Any(string.IsNullOrWhiteSpace).ShouldBeTrue();
    }

    [Fact]
    public void FilterableBuckets_ShouldContainUncatergorised_GivenAnyBucketList()
    {
        this.subject.FilterableBuckets().Any(b => b == TransactionConstants.UncategorisedFilter).ShouldBeTrue();
    }

    [Fact]
    public void FilterByBucket_ShouldReturn1Bucket_GivenUncatergorisedCode()
    {
        var model2 = new TransactionsListModelBuilder()
            .AppendTransaction(new Transaction { Account = TransactionsListModelTestData.ChequeAccount, Amount = -255.65M, BudgetBucket = null, Date = new DateOnly(2013, 9, 10) })
            .Merge(this.testData)
            .Build();
        this.testData = model2;
        Arrange();

        var result = this.subject.FilterByBucket(TransactionConstants.UncategorisedFilter);

        this.testData.Output(DateOnly.MinValue);
        result.Count().ShouldBe(1);
    }

    [Fact]
    public void FilterByBucket_ShouldReturn2Buckets_GivenSurplusBucketCode()
    {
        var model2 = new TransactionsListModelBuilder()
            .AppendTransaction(new Transaction
            {
                Account = TransactionsListModelTestData.ChequeAccount,
                Amount = -255.65M,
                BudgetBucket = TransactionsListModelTestData.SurplusBucket,
                Date = new DateOnly(2013, 9, 10)
            })
            .AppendTransaction(new Transaction
            {
                Account = TransactionsListModelTestData.ChequeAccount,
                Amount = -1000M,
                BudgetBucket = new FixedBudgetProjectBucket("FOO", "Bar", 2000M),
                Date = new DateOnly(2013, 9, 9)
            })
            .Merge(this.testData)
            .Build();
        this.testData = model2;
        this.budgetBucketRepo = new BudgetBucketRepoAlwaysFind();
        Arrange();

        var result = this.subject.FilterByBucket(SurplusBucket.SurplusCode);

        this.testData.Output(DateOnly.MinValue);
        result.Count().ShouldBe(2);
    }

    [Fact]
    public void FilterByBucket_ShouldReturn3Buckets_GivenIncomeBucketCode()
    {
        this.budgetBucketRepo = new BudgetBucketRepoAlwaysFind();
        Arrange();

        var result = this.subject.FilterByBucket(TransactionsListModelTestData.IncomeBucket.Code);

        result.Count().ShouldBe(3);
    }

    [Fact]
    public void FilterByBucket_ShouldReturnAllBuckets_GivenEmptyBucketCode()
    {
        this.budgetBucketRepo = new BudgetBucketRepoAlwaysFind();
        Arrange();

        var result = this.subject.FilterByBucket(string.Empty);

        result.Count().ShouldBe(10);
    }

    [Fact]
    public void FilterByBucket_ShouldReturnAllBuckets_GivenNullBucketCode()
    {
        this.budgetBucketRepo = new BudgetBucketRepoAlwaysFind();
        Arrange();

        var result = this.subject.FilterByBucket(null);

        result.Count().ShouldBe(10);
    }

    [Fact]
    public void FilterTransactions_ShouldCallTransactionsListModel_GivenFilterObject()
    {
        this.testData = new TransactionsListModelTestHarness();
        this.testData.LoadTransactions(new List<Transaction>());
        var criteria = new GlobalFilterCriteria { BeginDate = new DateOnly(2014, 07, 01), EndDate = new DateOnly(2014, 08, 01) };

        Arrange();

        this.subject.FilterTransactions(criteria);

        ((TransactionsListModelTestHarness)this.testData).FilterByCriteriaWasCalled.ShouldBe(1);
    }

    [Fact]
    public void FilterTransactions_ShouldThrow_GivenNullFilter()
    {
        Should.Throw<ArgumentNullException>(() => this.subject.FilterTransactions(null!));
    }

    [Fact]
    public async Task ImportAndMergeTransactionsExtract_ShouldCallTransactionsRepo_GivenStorageKeyAndAccount()
    {
        this.mockTransactionsRepo.ImportTransactionsExtractAsync(Arg.Any<string>(), Arg.Any<BankAccount.Account>())
            .Returns(Task.FromResult(TransactionsListModelTestData.TestData3()));

        await this.subject.ImportAndMergeTransactionsExtractAsync("Sticky Bag.csv", TransactionsListModelTestData.ChequeAccount);

        await this.mockTransactionsRepo.Received(1).ImportTransactionsExtractAsync(Arg.Any<string>(), Arg.Any<BankAccount.Account>());
    }

    [Fact]
    public async Task ImportAndMergeTransactionsExtract_ShouldMergeTheModel_GivenStorageKeyAndAccount()
    {
        this.testData = new TransactionsListModelTestHarness();
        this.testData.LoadTransactions(new List<Transaction>());
        Arrange();

        this.mockTransactionsRepo.ImportTransactionsExtractAsync(Arg.Any<string>(), Arg.Any<BankAccount.Account>())
            .Returns(Task.FromResult(TransactionsListModelTestData.TestData2()));

        await this.subject.ImportAndMergeTransactionsExtractAsync("Sticky Bag.csv", TransactionsListModelTestData.ChequeAccount);

        ((TransactionsListModelTestHarness)this.testData).MergeWasCalled.ShouldBe(1);
    }

    [Fact]
    public async Task ImportAndMergeTransactionsExtract_ShouldThrow_GivenAlreadyImported()
    {
        this.testData = new TransactionsListModelBuilder().TestData2().Build();
        Arrange();

        this.mockTransactionsRepo.ImportTransactionsExtractAsync(Arg.Any<string>(), Arg.Any<BankAccount.Account>())
            .Returns(Task.FromResult(TransactionsListModelTestData.TestData2()));

        await Should.ThrowAsync<TransactionsAlreadyImportedException>(async () =>
            await this.subject.ImportAndMergeTransactionsExtractAsync("Sticky Bag.csv", TransactionsListModelTestData.ChequeAccount));
    }

    [Fact]
    public async Task ImportAndMergeTransactionsExtract_ShouldThrow_GivenNullAccount()
    {
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await this.subject.ImportAndMergeTransactionsExtractAsync("Sticky Bag.csv", null!));
    }

    [Fact]
    public async Task ImportAndMergeTransactionsExtract_ShouldThrow_GivenNullStorageKey()
    {
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await this.subject.ImportAndMergeTransactionsExtractAsync(null!, new ChequeAccount("Foo")));
    }

    [Fact]
    public async Task LoadAsync_ShouldCallTransactionsRepo_GivenValidStorageKey()
    {
        await this.subject.LoadAsync(this.testAppDb);
        await this.mockTransactionsRepo.Received().LoadAsync(Arg.Any<string>(), Arg.Any<bool>());
    }

    [Fact]
    public async Task LoadAsync_ShouldReturnATransactionsModel_GivenValidStorageKey()
    {
        TransactionsListModel testTransactionsList = new TransactionsListModelTestHarness();
        testTransactionsList.LoadTransactions(new List<Transaction>());

        this.mockTransactionsRepo.LoadAsync(Arg.Any<string>(), Arg.Any<bool>()).Returns(Task.FromResult(testTransactionsList));

        await this.subject.LoadAsync(this.testAppDb);

        this.subject.TransactionsListModel.ShouldNotBeNull();
    }

    [Fact]
    public async Task LoadAsync_ShouldThrow_GivenNullStorageKey()
    {
        await Should.ThrowAsync<ArgumentNullException>(async () => await this.subject.LoadAsync(null!));
    }

    [Fact]
    public void RemoveTransaction_ShouldCallTransactionsModelRemove_GivenATransaction()
    {
        this.testData = new TransactionsListModelTestHarness();
        this.testData.LoadTransactions(TransactionsListModelTestData.TestData2().Transactions);
        Arrange();
        var transaction = this.testData.Transactions.Skip(1).First();

        this.subject.RemoveTransaction(transaction);

        ((TransactionsListModelTestHarness)this.testData).RemoveTransactionWasCalled.ShouldBe(1);
    }

    [Fact]
    public void RemoveTransaction_ShouldThrow_GivenNullTransaction()
    {
        Should.Throw<ArgumentNullException>(() => this.subject.RemoveTransaction(null!));
    }

    [Fact]
    public async Task Save_ShouldCallTransactionsRepo_GivenTransactionsModel()
    {
        await this.subject.SaveAsync(this.testAppDb);

        await this.mockTransactionsRepo.Received(1).SaveAsync(Arg.Any<TransactionsListModel>(), Arg.Any<bool>());
    }

    [Fact]
    public async Task Save_ShouldNotCallTransactionsRepo_GivenNullTransactionsModel()
    {
        this.subject = CreateSubject();
        await this.subject.SaveAsync(new ApplicationDatabase());

        await this.mockTransactionsRepo.DidNotReceive().SaveAsync(Arg.Any<TransactionsListModel>(), Arg.Any<bool>());
    }

    [Fact]
    public void SplitTransaction_ShouldCallTransactionsModel_GivenValidParams()
    {
        this.testData = new TransactionsListModelTestHarness();
        this.testData.LoadTransactions(new List<Transaction>());

        var transaction = new Transaction { Account = TransactionsListModelTestData.VisaAccount };

        Arrange();

        this.subject.SplitTransaction(transaction, 100, 200, TransactionsListModelTestData.CarMtcBucket, TransactionsListModelTestData.HairBucket);

        ((TransactionsListModelTestHarness)this.testData).SplitTransactionWasCalled.ShouldBe(1);
    }

    [Fact]
    public void TotalCount_ShouldBe10_GivenTestData2()
    {
        this.subject.TotalCount.ShouldBe(10);
    }

    [Fact]
    public void TotalCredits_ShouldBe2552_GivenTestData2()
    {
        this.subject.TotalCredits.ShouldBe(2552.97M);
    }

    [Fact]
    public void TotalDebits_ShouldBe806_GivenTestData2()
    {
        this.subject.TotalDebits.ShouldBe(-806.78M);
    }

    [Fact]
    public void TotalDifference_ShouldBe1746_GivenTestData2()
    {
        (this.subject.TotalCredits + this.subject.TotalDebits).ShouldBe(1746.19M);
    }

    private void Arrange()
    {
        this.mockTransactionsRepo
            .LoadAsync(Arg.Any<string>(), Arg.Any<bool>())
            .Returns(Task.FromResult(this.testData));
        this.subject = CreateSubject();
        this.subject.LoadAsync(this.testAppDb).Wait();
    }

    private TransactionManagerService CreateSubject()
    {
        return new TransactionManagerService(this.budgetBucketRepo, this.mockTransactionsRepo, new FakeLogger(), new FakeMonitorableDependencies());
    }
}
