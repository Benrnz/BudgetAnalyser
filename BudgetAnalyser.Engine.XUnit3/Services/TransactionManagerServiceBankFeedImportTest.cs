using System.Text;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.BankFeed;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Engine.Transactions.Data;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Services;

/// <summary>
///     Tests downloading and merging bank feed transactions through <see cref="TransactionManagerService" />, using a fake <see cref="IBankFeedProvider" />.
/// </summary>
public class TransactionManagerServiceBankFeedImportTest
{
    private const string ProviderKey = "test.provider";
    private static readonly ChequeAccount Cheque = new ChequeAccount(AccountTypeRepositoryConstants.Cheque);
    private static readonly ImportDateRange Range = new(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31));
    private static readonly VisaAccount Visa = new VisaAccount(AccountTypeRepositoryConstants.Visa);

    private readonly BankFeedAccountMapping chequeMapping = new("acc_test_cheque", Cheque, BankFeedFieldMapping.Default);
    private readonly IBankFeedConfigurationService configurationService = Substitute.For<IBankFeedConfigurationService>();
    private readonly IBankFeedProvider provider = Substitute.For<IBankFeedProvider>();
    private readonly TransactionManagerService subject;
    private readonly BankFeedAccountMapping visaMapping = new("acc_test_visa", Visa, BankFeedFieldMapping.Default);
    private readonly ITransactionsListModelRepository transactionsRepo = Substitute.For<ITransactionsListModelRepository>();

    public TransactionManagerServiceBankFeedImportTest()
    {
        this.provider.ProviderKey.Returns(ProviderKey);
        ArrangeFeed(this.chequeMapping, FeedTransaction("trans_test_c1", "acc_test_cheque", 3, -10M), FeedTransaction("trans_test_c2", "acc_test_cheque", 4, -20M));
        ArrangeFeed(this.visaMapping, FeedTransaction("trans_test_v1", "acc_test_visa", 5, -30M));

        var importer = new BankFeedTransactionsImporter(this.provider, this.configurationService, new InMemoryTransactionTypeRepository(), new FakeLogger());
        this.subject = new TransactionManagerService(Substitute.For<IBudgetBucketRepository>(), this.transactionsRepo, new FakeLogger(), new FakeMonitorableDependencies(), importer);
    }

    [Fact]
    public async Task Import_ShouldAddNewTransactionsAndReportCounts()
    {
        await LoadExistingTransactions(ExistingTransaction(new DateOnly(2026, 9, 30), -99M));

        var result = await this.subject.ImportAndMergeBankFeedTransactionsAsync(Range, [this.chequeMapping, this.visaMapping], CancellationToken.None);

        result.TotalImported.ShouldBe(3);
        result.Range.ShouldBe(Range);
        result.Accounts.Count.ShouldBe(2);
        result.Accounts[0].ShouldBe(new BankFeedAccountImportResult(Cheque, 2, 2, 0));
        result.Accounts[1].ShouldBe(new BankFeedAccountImportResult(Visa, 1, 1, 0));
        result.SuspectedDuplicates.ShouldBe(0);
        this.subject.TransactionsListModel!.AllTransactions.Count().ShouldBe(4);
    }

    [Fact]
    public async Task Import_ShouldBeNothingNew_GivenEmptyDownload()
    {
        await LoadExistingTransactions();
        var original = this.subject.TransactionsListModel;
        this.provider.GetTransactionsAsync("acc_test_cheque", Range, Arg.Any<CancellationToken>()).Returns([]);

        var result = await this.subject.ImportAndMergeBankFeedTransactionsAsync(Range, [this.chequeMapping], CancellationToken.None);

        result.TotalImported.ShouldBe(0);
        result.Accounts.Single().Downloaded.ShouldBe(0);
        this.subject.TransactionsListModel.ShouldBeSameAs(original);
    }

    [Fact]
    public async Task Import_ShouldFlagSuspectedDuplicates_GivenSameTransactionImportedFromCsvBefore()
    {
        // A CSV import of the same bank transaction has a random Id, so it is only caught by the existing hash check.
        await LoadExistingTransactions(ExistingTransaction(new DateOnly(2026, 10, 3), -10M, "TEST DESCRIPTION trans_test_c1"));

        var result = await this.subject.ImportAndMergeBankFeedTransactionsAsync(Range, [this.chequeMapping], CancellationToken.None);

        result.TotalImported.ShouldBe(2);
        result.SuspectedDuplicates.ShouldBe(2);
    }

    [Fact]
    public async Task Import_ShouldImportNothing_GivenSecondAccountFails()
    {
        await LoadExistingTransactions();
        var original = this.subject.TransactionsListModel;
        this.provider.GetTransactionsAsync("acc_test_visa", Range, Arg.Any<CancellationToken>()).ThrowsAsync(new BankFeedException("Test failure"));

        await Should.ThrowAsync<BankFeedException>(() => this.subject.ImportAndMergeBankFeedTransactionsAsync(Range, [this.chequeMapping, this.visaMapping], CancellationToken.None));

        this.subject.TransactionsListModel.ShouldBeSameAs(original);
        this.subject.TransactionsListModel!.AllTransactions.ShouldBeEmpty();
    }

    [Fact]
    public async Task Import_ShouldImportOnlyNewTransactions_GivenPartialOverlap()
    {
        await LoadExistingTransactions();
        await this.subject.ImportAndMergeBankFeedTransactionsAsync(Range, [this.chequeMapping], CancellationToken.None);
        ArrangeFeed(
            this.chequeMapping,
            FeedTransaction("trans_test_c2", "acc_test_cheque", 4, -20M),
            FeedTransaction("trans_test_c3", "acc_test_cheque", 6, -40M));

        var result = await this.subject.ImportAndMergeBankFeedTransactionsAsync(Range, [this.chequeMapping], CancellationToken.None);

        result.Accounts.Single().ShouldBe(new BankFeedAccountImportResult(Cheque, 2, 1, 1));
        this.subject.TransactionsListModel!.AllTransactions.Count().ShouldBe(3);
    }

    [Fact]
    public async Task Import_ShouldNeverAddDuplicateIds_GivenFeedRepeatsATransaction()
    {
        await LoadExistingTransactions();
        ArrangeFeed(
            this.chequeMapping,
            FeedTransaction("trans_test_c1", "acc_test_cheque", 3, -10M),
            FeedTransaction("trans_test_c1", "acc_test_cheque", 3, -10M));

        var result = await this.subject.ImportAndMergeBankFeedTransactionsAsync(Range, [this.chequeMapping], CancellationToken.None);

        result.Accounts.Single().ShouldBe(new BankFeedAccountImportResult(Cheque, 2, 1, 1));
        var ids = this.subject.TransactionsListModel!.AllTransactions.Select(t => t.Id).ToList();
        ids.Distinct().Count().ShouldBe(ids.Count);
    }

    [Fact]
    public async Task Import_ShouldOnlyDownloadSelectedMappings()
    {
        await LoadExistingTransactions();

        await this.subject.ImportAndMergeBankFeedTransactionsAsync(Range, [this.visaMapping], CancellationToken.None);

        await this.provider.DidNotReceive().GetTransactionsAsync("acc_test_cheque", Arg.Any<ImportDateRange>(), Arg.Any<CancellationToken>());
        this.subject.TransactionsListModel!.AllTransactions.ShouldAllBe(t => t.Account == Visa);
    }

    [Fact]
    public async Task Import_ShouldResetVerifiedConnection_GivenCredentialsRejected()
    {
        await LoadExistingTransactions();
        this.provider.GetTransactionsAsync("acc_test_cheque", Range, Arg.Any<CancellationToken>()).ThrowsAsync(new BankFeedAuthenticationException("Rejected"));

        await Should.ThrowAsync<BankFeedAuthenticationException>(() => this.subject.ImportAndMergeBankFeedTransactionsAsync(Range, [this.chequeMapping], CancellationToken.None));

        this.configurationService.Received(1).NotifyCredentialsRejected();
    }

    [Fact]
    public async Task Import_ShouldSkipEverything_GivenSameRangeImportedTwice()
    {
        await LoadExistingTransactions();
        await this.subject.ImportAndMergeBankFeedTransactionsAsync(Range, [this.chequeMapping, this.visaMapping], CancellationToken.None);
        var afterFirstImport = this.subject.TransactionsListModel;

        var result = await this.subject.ImportAndMergeBankFeedTransactionsAsync(Range, [this.chequeMapping, this.visaMapping], CancellationToken.None);

        result.TotalImported.ShouldBe(0);
        result.Accounts.Sum(a => a.AlreadyImported).ShouldBe(3);
        this.subject.TransactionsListModel.ShouldBeSameAs(afterFirstImport);
        this.subject.TransactionsListModel!.AllTransactions.Count().ShouldBe(3);
    }

    [Fact]
    public async Task Import_ShouldThrow_GivenNoMappingsSelected()
    {
        await LoadExistingTransactions();

        await Should.ThrowAsync<ArgumentException>(() => this.subject.ImportAndMergeBankFeedTransactionsAsync(Range, [], CancellationToken.None));
    }

    [Fact]
    public async Task Import_ShouldThrow_GivenNoStartDate()
    {
        await LoadExistingTransactions();

        await Should.ThrowAsync<ArgumentException>(() =>
            this.subject.ImportAndMergeBankFeedTransactionsAsync(new ImportDateRange(null, Range.End), [this.chequeMapping], CancellationToken.None));
    }

    [Fact]
    public async Task Import_ShouldThrow_GivenNoTransactionsLoaded()
    {
        await Should.ThrowAsync<InvalidOperationException>(() => this.subject.ImportAndMergeBankFeedTransactionsAsync(Range, [this.chequeMapping], CancellationToken.None));
    }

    [Fact]
    public async Task ImportedIds_ShouldSurviveSaveAndReloadThroughCsv()
    {
        await LoadExistingTransactions();
        await this.subject.ImportAndMergeBankFeedTransactionsAsync(Range, [this.chequeMapping, this.visaMapping], CancellationToken.None);
        var expectedIds = this.subject.TransactionsListModel!.AllTransactions.Select(t => t.Id).OrderBy(id => id).ToList();

        var readerWriter = Substitute.For<IFileReaderWriter>();
        var selector = Substitute.For<IReaderWriterSelector>();
        selector.SelectReaderWriter(Arg.Any<bool>()).Returns(readerWriter);
        var logger = new FakeLogger();
        var mapper = new MapperTransactionsListModelToDto2(new InMemoryAccountTypeRepository(), new BudgetBucketRepoAlwaysFind(), new InMemoryTransactionTypeRepository(), logger);
        var csvRepository = new CsvOnDiskTransactionsModelRepositoryV2TestHarness(logger, mapper, selector);
        this.subject.TransactionsListModel.StorageKey = "Test.csv";
        await csvRepository.SaveAsync(this.subject.TransactionsListModel, false);
        readerWriter.CreateReadableStream(Arg.Any<string>()).Returns(new MemoryStream(Encoding.UTF8.GetBytes(csvRepository.SerialisedData)));

        var reloaded = await csvRepository.LoadAsync("Test.csv", false);

        reloaded.AllTransactions.Select(t => t.Id).OrderBy(id => id).ShouldBe(expectedIds);
        reloaded.AllTransactions.ShouldAllBe(t => t.Id.Version == 5);
    }

    private static Transaction ExistingTransaction(DateOnly date, decimal amount, string description = "TEST EXISTING")
    {
        return new Transaction { Account = Cheque, Date = date, Amount = amount, Description = description };
    }

    private static BankFeedTransaction FeedTransaction(string id, string accountId, int day, decimal amount)
    {
        return new BankFeedTransaction(id, accountId, "Test Bank A", new DateOnly(2026, 10, day), $"TEST DESCRIPTION {id}", amount, null, null, null, null, null, null, null);
    }

    private void ArrangeFeed(BankFeedAccountMapping mapping, params BankFeedTransaction[] transactions)
    {
        this.provider.GetTransactionsAsync(mapping.ExternalAccountId, Range, Arg.Any<CancellationToken>()).Returns(transactions);
    }

    private async Task LoadExistingTransactions(params Transaction[] existing)
    {
        var model = new TransactionsListModel(new FakeLogger()).LoadTransactions(existing);
        this.transactionsRepo.LoadAsync(Arg.Any<string>(), Arg.Any<bool>()).Returns(model);
        await this.subject.LoadAsync(new ApplicationDatabase { FileName = @"C:\Test\Test.bax", TransactionsListModelStorageKey = "Test.csv" });
    }
}
