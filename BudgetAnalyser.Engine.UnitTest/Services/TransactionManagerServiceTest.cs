using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.UnitTest.Helper;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Moq;
using Rees.UnitTestUtilities;

namespace BudgetAnalyser.Engine.UnitTest.Services;

[TestClass]
public class TransactionManagerServiceTest
{
    private readonly ApplicationDatabase testAppDb = new();
    private IBudgetBucketRepository budgetBucketRepo; // By default set to return mockBudgetBucketRepo.Object;
    private Mock<IBudgetBucketRepository> mockBudgetBucketRepo;
    private Mock<ITransactionsListModelRepository> mockTransactionsRepo;
    private TransactionManagerService subject;
    private TransactionsListModel testData;

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Ctor_ShouldThrow_GivenNullBucketRepo()
    {
        new TransactionManagerService(null, this.mockTransactionsRepo.Object, new FakeLogger(), new FakeMonitorableDependencies());
        Assert.Fail();
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Ctor_ShouldThrow_GivenNullLogger()
    {
        new TransactionManagerService(this.mockBudgetBucketRepo.Object, this.mockTransactionsRepo.Object, null, new FakeMonitorableDependencies());
        Assert.Fail();
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Ctor_ShouldThrow_GivenNullTransactionsRepo()
    {
        new TransactionManagerService(this.mockBudgetBucketRepo.Object, null, new FakeLogger(), new FakeMonitorableDependencies());
        Assert.Fail();
    }

    [TestMethod]
    public void DetectDuplicateTransactions_ShouldReturnEmpty_GivenNullTransactionsModel()
    {
        this.subject = CreateSubject();
        Assert.AreEqual(string.Empty, this.subject.DetectDuplicateTransactions());
    }

    [TestMethod]
    public void DetectDuplicateTransactions_ShouldReturnEmpty_GivenTestData2()
    {
        Assert.IsTrue(string.IsNullOrWhiteSpace(this.subject.DetectDuplicateTransactions()));
    }

    [TestMethod]
    public void DetectDuplicateTransactions_ShouldSummaryText_GivenTestData4()
    {
        this.testData = TransactionsListModelTestData.TestData4();
        Arrange();

        var result = this.subject.DetectDuplicateTransactions();
        Console.WriteLine(result);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result));
    }

    [TestMethod]
    public void FilterableBuckets_ShouldContainABlankElement_GivenAnyBucketList()
    {
        Assert.IsTrue(this.subject.FilterableBuckets().Any(string.IsNullOrWhiteSpace));
    }

    [TestMethod]
    public void FilterableBuckets_ShouldContainUncatergorised_GivenAnyBucketList()
    {
        Assert.IsTrue(this.subject.FilterableBuckets().Any(b => b == TransactionConstants.UncategorisedFilter));
    }

    [TestMethod]
    public void FilterByBucket_ShouldReturn1Bucket_GivenUncatergorisedCode()
    {
        var model2 = new TransactionsListModelBuilder()
            .AppendTransaction(
                new Transaction { Account = TransactionsListModelTestData.ChequeAccount, Amount = -255.65M, BudgetBucket = null, Date = new DateOnly(2013, 9, 10) })
            .Merge(this.testData)
            .Build();
        this.testData = model2;
        Arrange();

        var result = this.subject.FilterByBucket(TransactionConstants.UncategorisedFilter);

        this.testData.Output(DateOnly.MinValue);
        Assert.AreEqual(1, result.Count());
    }

    [TestMethod]
    public void FilterByBucket_ShouldReturn2Buckets_GivenSurplusBucketCode()
    {
        var model2 = new TransactionsListModelBuilder()
            .AppendTransaction(
                new Transaction { Account = TransactionsListModelTestData.ChequeAccount, Amount = -255.65M, BudgetBucket = TransactionsListModelTestData.SurplusBucket, Date = new DateOnly(2013, 9, 10) })
            .AppendTransaction(
                new Transaction { Account = TransactionsListModelTestData.ChequeAccount, Amount = -1000M, BudgetBucket = new FixedBudgetProjectBucket("FOO", "Bar", 2000M), Date = new DateOnly(2013, 9, 9) })
            .Merge(this.testData)
            .Build();
        this.testData = model2;
        this.budgetBucketRepo = new BucketBucketRepoAlwaysFind();
        Arrange();

        var result = this.subject.FilterByBucket(SurplusBucket.SurplusCode);

        this.testData.Output(DateOnly.MinValue);
        Assert.AreEqual(2, result.Count());
    }

    [TestMethod]
    public void FilterByBucket_ShouldReturn3Buckets_GivenIncomeBucketCode()
    {
        this.budgetBucketRepo = new BucketBucketRepoAlwaysFind();
        Arrange();

        var result = this.subject.FilterByBucket(TransactionsListModelTestData.IncomeBucket.Code);

        Assert.AreEqual(3, result.Count());
    }

    [TestMethod]
    public void FilterByBucket_ShouldReturnAllBuckets_GivenEmptyBucketCode()
    {
        this.budgetBucketRepo = new BucketBucketRepoAlwaysFind();
        Arrange();

        var result = this.subject.FilterByBucket(string.Empty);

        Assert.AreEqual(10, result.Count());
    }

    [TestMethod]
    public void FilterByBucket_ShouldReturnAllBuckets_GivenNullBucketCode()
    {
        this.budgetBucketRepo = new BucketBucketRepoAlwaysFind();
        Arrange();

        var result = this.subject.FilterByBucket(null);

        Assert.AreEqual(10, result.Count());
    }

    [TestMethod]
    public void FilterTransactions_ShouldCallTransactionsListModel_GivenFilterObject()
    {
        this.testData = new TransactionsListModelTestHarness();
        this.testData.LoadTransactions(new List<Transaction>());
        var criteria = new GlobalFilterCriteria { BeginDate = new DateOnly(2014, 07, 01), EndDate = new DateOnly(2014, 08, 01) };

        Arrange();

        this.subject.FilterTransactions(criteria);

        Assert.AreEqual(1, ((TransactionsListModelTestHarness)this.testData).FilterByCriteriaWasCalled);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void FilterTransactions_ShouldThrow_GivenNullFilter()
    {
        this.subject.FilterTransactions(null);
        Assert.Fail();
    }

    [TestMethod]
    public async Task ImportAndMergeBankStatement_ShouldCallTransactionsRepo_GivenStorageKeyAndAccount()
    {
        this.mockTransactionsRepo
            .Setup(m => m.ImportTransactionsExtractAsync(It.IsAny<string>(), It.IsAny<BankAccount.Account>()))
            .Returns(Task.FromResult(TransactionsListModelTestData.TestData3()))
            .Verifiable();

        await this.subject.ImportAndMergeTransactionsExtractAsync("Sticky Bag.csv", TransactionsListModelTestData.ChequeAccount);

        this.mockTransactionsRepo.Verify();
    }

    [TestMethod]
    public async Task ImportAndMergeBankStatement_ShouldMergeTheModel_GivenStorageKeyAndAccount()
    {
        this.testData = new TransactionsListModelTestHarness();
        this.testData.LoadTransactions(new List<Transaction>());

        Arrange();

        this.mockTransactionsRepo
            .Setup(m => m.ImportTransactionsExtractAsync(It.IsAny<string>(), It.IsAny<BankAccount.Account>()))
            .Returns(Task.FromResult(TransactionsListModelTestData.TestData2()))
            .Verifiable();

        await this.subject.ImportAndMergeTransactionsExtractAsync("Sticky Bag.csv", TransactionsListModelTestData.ChequeAccount);

        Assert.AreEqual(1, ((TransactionsListModelTestHarness)this.testData).MergeWasCalled);
    }

    [TestMethod]
    [ExpectedException(typeof(TransactionsAlreadyImportedException))]
    public async Task ImportAndMergeBankStatement_ShouldThrow_GivenAlreadyImported()
    {
        this.testData = new TransactionsListModelBuilder()
            .TestData2()
            .Build();

        Arrange();

        this.mockTransactionsRepo
            .Setup(m => m.ImportTransactionsExtractAsync(It.IsAny<string>(), It.IsAny<BankAccount.Account>()))
            .Returns(Task.FromResult(TransactionsListModelTestData.TestData2()))
            .Verifiable();

        await this.subject.ImportAndMergeTransactionsExtractAsync("Sticky Bag.csv", TransactionsListModelTestData.ChequeAccount);

        Assert.Fail();
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task ImportAndMergeBankStatement_ShouldThrow_GivenNullAccount()
    {
        await this.subject.ImportAndMergeTransactionsExtractAsync("Sticky Bag.csv", null);
        Assert.Fail();
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task ImportAndMergeBankStatement_ShouldThrow_GivenNullStorageKey()
    {
        await this.subject.ImportAndMergeTransactionsExtractAsync(null, new ChequeAccount("Foo"));
        Assert.Fail();
    }

    [TestMethod]
    public async Task LoadStatementModelAsync_ShouldCallTransactionsRepo_GivenValidStorageKey()
    {
        await this.subject.LoadAsync(this.testAppDb);
        this.mockTransactionsRepo.Verify();
    }

    [TestMethod]
    public async Task LoadStatementModelAsync_ShouldReturnATransactionsModel_GivenValidStorageKey()
    {
        TransactionsListModel testTransactionsList = new TransactionsListModelTestHarness();
        testTransactionsList.LoadTransactions(new List<Transaction>());

        this.mockTransactionsRepo.Setup(m => m.LoadAsync(It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(testTransactionsList));

        await this.subject.LoadAsync(this.testAppDb);

        Assert.IsNotNull(this.subject.TransactionsListModel);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task LoadStatementModelAsync_ShouldThrow_GivenNullStorageKey()
    {
        await this.subject.LoadAsync(null);
        Assert.Fail();
    }

    [TestMethod]
    public void RemoveTransaction_ShouldCallTransactionsModelRemove_GivenATransaction()
    {
        this.testData = new TransactionsListModelTestHarness();
        this.testData.LoadTransactions(TransactionsListModelTestData.TestData2().Transactions);
        Arrange();
        var transaction = this.testData.Transactions.Skip(1).First();

        this.subject.RemoveTransaction(transaction);

        Assert.AreEqual(1, ((TransactionsListModelTestHarness)this.testData).RemoveTransactionWasCalled);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void RemoveTransaction_ShouldThrow_GivenNullTransaction()
    {
        this.subject.RemoveTransaction(null);

        Assert.Fail();
    }

    [TestMethod]
    public async Task Save_ShouldCallTransactionsRepo_GivenTransactionsModel()
    {
        var mockAppDb = new Mock<ApplicationDatabase>();
        mockAppDb.Setup(m => m.FullPath(It.IsAny<string>())).Returns("Foo");
        await this.subject.SaveAsync(mockAppDb.Object);

        this.mockTransactionsRepo.Verify(m => m.SaveAsync(It.IsAny<TransactionsListModel>(), It.IsAny<bool>()), Times.Once);
    }

    [TestMethod]
    public async Task Save_ShouldNotCallTransactionsRepo_GivenNullTransactionsModel()
    {
        this.subject = CreateSubject();
        await this.subject.SaveAsync(It.IsAny<ApplicationDatabase>());

        this.mockTransactionsRepo.Verify(m => m.SaveAsync(It.IsAny<TransactionsListModel>(), It.IsAny<bool>()), Times.Never);
    }

    [TestMethod]
    public void SplitTransaction_ShouldCallTransactionsModel_GivenValidParams()
    {
        this.testData = new TransactionsListModelTestHarness();
        this.testData.LoadTransactions(new List<Transaction>());

        var transaction = new Transaction { Account = TransactionsListModelTestData.VisaAccount };

        Arrange();

        this.subject.SplitTransaction(transaction, 100, 200, TransactionsListModelTestData.CarMtcBucket, TransactionsListModelTestData.HairBucket);

        Assert.AreEqual(1, ((TransactionsListModelTestHarness)this.testData).SplitTransactionWasCalled);
    }

    [TestInitialize]
    public void TestInit()
    {
        PrivateAccessor.SetProperty(this.testAppDb, "StatementModelStorageKey", @"Foo.csv");
        PrivateAccessor.SetProperty(this.testAppDb, "FileName", @"C:\AppDb.bax");
        this.testData = TransactionsListModelTestData.TestData2();

        this.mockBudgetBucketRepo = new Mock<IBudgetBucketRepository>();
        this.mockTransactionsRepo = new Mock<ITransactionsListModelRepository>();

        this.mockTransactionsRepo.Setup(m => m.LoadAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(Task.FromResult(this.testData))
            .Verifiable();

        this.mockBudgetBucketRepo
            .Setup(m => m.Buckets)
            .Returns(BudgetBucketTestData.BudgetModelTestData1Buckets);

        this.budgetBucketRepo = this.mockBudgetBucketRepo.Object;
        Arrange();
    }

    [TestMethod]
    public void TotalCount_ShouldBe10_GivenTestData2()
    {
        Assert.AreEqual(10, this.subject.TotalCount);
    }

    [TestMethod]
    public void TotalCredits_ShouldBe2552_GivenTestData2()
    {
        Assert.AreEqual(2552.97M, this.subject.TotalCredits);
    }

    [TestMethod]
    public void TotalDebits_ShouldBe806_GivenTestData2()
    {
        Assert.AreEqual(-806.78M, this.subject.TotalDebits);
    }

    [TestMethod]
    public void TotalDifference_ShouldBe1746_GivenTestData2()
    {
        Assert.AreEqual(1746.19M, this.subject.TotalCredits + this.subject.TotalDebits);
    }

    private void Arrange()
    {
        this.mockTransactionsRepo
            .Setup(m => m.LoadAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(Task.FromResult(this.testData));
        this.subject = CreateSubject();
        this.subject.LoadAsync(this.testAppDb).Wait();
    }

    private TransactionManagerService CreateSubject()
    {
        return new TransactionManagerService(this.budgetBucketRepo, this.mockTransactionsRepo.Object, new FakeLogger(), new FakeMonitorableDependencies());
    }
}
