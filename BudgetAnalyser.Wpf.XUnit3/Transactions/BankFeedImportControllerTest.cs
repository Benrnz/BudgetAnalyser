using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.BankFeed;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using BudgetAnalyser.ShellDialog;
using BudgetAnalyser.Transactions;
using CommunityToolkit.Mvvm.Messaging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace BudgetAnalyser.Wpf.XUnit3.Transactions;

public class BankFeedImportControllerTest
{
    private static readonly ChequeAccount Cheque = new(AccountTypeRepositoryConstants.Cheque);
    private static readonly BankFeedConnectionStatus Connected = new(true, "test.user@example.com", new DateOnly(2024, 11, 20));
    private static readonly BankFeedAccount FeedCheque = new("acc_test_1", "Test Bank A", "Test Everyday", null, true, DateTimeOffset.Parse("2026-11-19T18:00:00Z"));
    private static readonly BankFeedAccount FeedVisa = new("acc_test_2", "Test Bank B", "Test Card", null, true, DateTimeOffset.Parse("2026-11-19T18:00:00Z"));

    // Friday 20 November 2026, so the last business day is Thursday 19 November.
    private static readonly DateOnly Today = new(2026, 11, 20);
    private static readonly VisaAccount Visa = new(AccountTypeRepositoryConstants.Visa);

    private readonly IBankFeedConfigurationService configuration = Substitute.For<IBankFeedConfigurationService>();
    private readonly WeakReferenceMessenger messenger = new();
    private readonly BankFeedImportController subject;

    public BankFeedImportControllerTest()
    {
        this.configuration.Mappings.Returns([
            new BankFeedAccountMapping("acc_test_1", Cheque, BankFeedFieldMapping.Default),
            new BankFeedAccountMapping("acc_test_2", Visa, BankFeedFieldMapping.Default)
        ]);
        this.configuration.GetFeedAccountsAsync(Arg.Any<CancellationToken>()).Returns([FeedCheque, FeedVisa]);
        this.configuration.TestConnectionAsync(Arg.Any<CancellationToken>()).Returns(Connected);
        this.subject = new BankFeedImportController(this.messenger, this.configuration, new FakeLogger());
    }

    [Fact]
    public void ApplyFeedDetails_ShouldUntickAndDisable_GivenMappingNotInFeed()
    {
        PrepareWithLastTransaction(new DateOnly(2026, 11, 12));

        this.subject.ApplyFeedDetails([FeedCheque], Connected);

        var visa = this.subject.AccountRows.Single(r => r.AccountName == AccountTypeRepositoryConstants.Visa);
        visa.IsSelected.ShouldBeFalse();
        visa.IsAvailable.ShouldBeFalse();
        visa.StatusText.ShouldContain("not found");
        this.subject.CreateRequest()!.Mappings.Single().ExternalAccountId.ShouldBe("acc_test_1");
    }

    [Fact]
    public void ApplyFeedDetails_ShouldUntickAndDisable_GivenInactiveAccount()
    {
        PrepareWithLastTransaction(new DateOnly(2026, 11, 12));

        this.subject.ApplyFeedDetails([FeedCheque, FeedVisa with { IsActive = false }], Connected);

        var visa = this.subject.AccountRows.Single(r => r.AccountName == AccountTypeRepositoryConstants.Visa);
        visa.IsSelected.ShouldBeFalse();
        visa.StatusText.ShouldContain("not currently connected");
    }

    [Fact]
    public void CreateRequest_ShouldReturnRangeAndTickedAccounts()
    {
        PrepareWithLastTransaction(new DateOnly(2026, 11, 12));
        this.subject.ApplyFeedDetails([FeedCheque, FeedVisa], Connected);
        this.subject.AccountRows[1].IsSelected = false;

        var request = this.subject.CreateRequest()!;

        request.Range.ShouldBe(new ImportDateRange(new DateOnly(2026, 11, 13), new DateOnly(2026, 11, 19)));
        request.Mappings.Single().Account.ShouldBe(Cheque);
    }

    [Fact]
    public void OverlapWarning_ShouldNameAccount_GivenFromOnOrBeforeLatestTransaction()
    {
        PrepareWithLastTransaction(new DateOnly(2026, 11, 12));
        this.subject.ApplyFeedDetails([FeedCheque, FeedVisa], Connected);

        this.subject.FromDate = new DateOnly(2026, 11, 12);

        this.subject.OverlapWarning.ShouldContain(AccountTypeRepositoryConstants.Cheque);
        this.subject.OverlapWarning.ShouldNotContain(AccountTypeRepositoryConstants.Visa);
        this.subject.CanExecuteOkButton.ShouldBeTrue(); // A warning, not a block.
    }

    [Fact]
    public void OverlapWarning_ShouldBeEmpty_GivenSuggestedRange()
    {
        PrepareWithLastTransaction(new DateOnly(2026, 11, 12));

        this.subject.OverlapWarning.ShouldBeEmpty();
    }

    [Fact]
    public void Prepare_ShouldLeaveFromBlank_GivenNoTransactions()
    {
        this.subject.Prepare(new TransactionsListModel(new FakeLogger()).LoadTransactions([]), Today);

        this.subject.FromDate.ShouldBeNull();
        this.subject.ToDate.ShouldBe(new DateOnly(2026, 11, 19));
        this.subject.ValidationMessage.ShouldBe("Choose a From date.");
        this.subject.CanExecuteOkButton.ShouldBeFalse();
    }

    [Fact]
    public void Prepare_ShouldSuggestRangeAndTickAllAccounts()
    {
        PrepareWithLastTransaction(new DateOnly(2026, 11, 12));

        this.subject.FromDate.ShouldBe(new DateOnly(2026, 11, 13));
        this.subject.ToDate.ShouldBe(new DateOnly(2026, 11, 19));
        this.subject.AccountRows.Count.ShouldBe(2);
        this.subject.AccountRows.ShouldAllBe(r => r.IsSelected);
    }

    [Fact]
    public async Task RequestUserInput_ShouldDisableOkUntilFeedChecked_ThenReturnChoice()
    {
        var transactions = CreateTransactions(new DateOnly(2026, 11, 12));
        var accountsSource = new TaskCompletionSource<IReadOnlyList<BankFeedAccount>>();
        this.configuration.GetFeedAccountsAsync(Arg.Any<CancellationToken>()).Returns(accountsSource.Task);
        ShellDialogRequestMessage? dialog = null;
        var recipient = new object();
        this.messenger.Register<object, ShellDialogRequestMessage>(recipient, (_, m) => dialog = m);

        var resultTask = this.subject.RequestUserInputAsync(transactions, Today);
        this.subject.IsLoading.ShouldBeTrue();
        this.subject.CanExecuteOkButton.ShouldBeFalse();
        accountsSource.SetResult([FeedCheque, FeedVisa]);
        await WaitUntil(() => !this.subject.IsLoading);
        this.subject.CanExecuteOkButton.ShouldBeTrue();
        this.messenger.Send(new ShellDialogResponseMessage(this.subject, ShellDialogButton.Ok) { CorrelationId = dialog!.CorrelationId });

        var result = await resultTask;

        result.ShouldNotBeNull();
        result.Mappings.Count.ShouldBe(2);
        dialog.Title.ShouldBe("Download from bank feed");
    }

    [Fact]
    public async Task RequestUserInput_ShouldReturnNull_GivenCancel()
    {
        ShellDialogRequestMessage? dialog = null;
        var recipient = new object();
        this.messenger.Register<object, ShellDialogRequestMessage>(recipient, (_, m) => dialog = m);

        var resultTask = this.subject.RequestUserInputAsync(CreateTransactions(new DateOnly(2026, 11, 12)), Today);
        await WaitUntil(() => !this.subject.IsLoading);
        this.messenger.Send(new ShellDialogResponseMessage(this.subject, ShellDialogButton.Cancel) { CorrelationId = dialog!.CorrelationId });

        (await resultTask).ShouldBeNull();
    }

    [Fact]
    public async Task RequestUserInput_ShouldShowRejectedMessage_GivenCredentialsRejected()
    {
        this.configuration.GetFeedAccountsAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new BankFeedAuthenticationException("Rejected"));
        ShellDialogRequestMessage? dialog = null;
        var recipient = new object();
        this.messenger.Register<object, ShellDialogRequestMessage>(recipient, (_, m) => dialog = m);

        var resultTask = this.subject.RequestUserInputAsync(CreateTransactions(new DateOnly(2026, 11, 12)), Today);
        await WaitUntil(() => !this.subject.IsLoading);

        this.subject.LoadError.ShouldBe("Your bank feed credentials were rejected. Update them on the Settings tab.");
        this.subject.CanExecuteOkButton.ShouldBeFalse();
        this.messenger.Send(new ShellDialogResponseMessage(this.subject, ShellDialogButton.Cancel) { CorrelationId = dialog!.CorrelationId });
        (await resultTask).ShouldBeNull();
    }

    [Fact]
    public void StaleWarning_ShouldFlagAccount_GivenDataRefreshedBeforeEndOfToDay()
    {
        PrepareWithLastTransaction(new DateOnly(2026, 11, 12));
        // Refreshed at 6am on the To date, before that day's transactions are complete.
        var staleVisa = FeedVisa with { DataAsOf = new DateTimeOffset(new DateTime(2026, 11, 19, 6, 0, 0, DateTimeKind.Local)) };
        var freshCheque = FeedCheque with { DataAsOf = new DateTimeOffset(new DateTime(2026, 11, 20, 6, 0, 0, DateTimeKind.Local)) };

        this.subject.ApplyFeedDetails([freshCheque, staleVisa], Connected);

        this.subject.AccountRows.Single(r => r.AccountName == AccountTypeRepositoryConstants.Visa).IsStale.ShouldBeTrue();
        this.subject.AccountRows.Single(r => r.AccountName == AccountTypeRepositoryConstants.Cheque).IsStale.ShouldBeFalse();
        this.subject.StaleWarning.ShouldContain("Test Bank B");
        this.subject.StaleWarning.ShouldNotContain("Test Bank A");
    }

    [Fact]
    public void Validation_ShouldBlock_GivenFromAfterTo()
    {
        PrepareWithLastTransaction(new DateOnly(2026, 11, 12));

        this.subject.FromDate = new DateOnly(2026, 11, 19);
        this.subject.ToDate = new DateOnly(2026, 11, 18);

        this.subject.ValidationMessage.ShouldBe("From must be on or before To.");
        this.subject.CanExecuteOkButton.ShouldBeFalse();
    }

    [Fact]
    public void Validation_ShouldBlock_GivenFromBeforeHistoryAvailable()
    {
        PrepareWithLastTransaction(new DateOnly(2026, 11, 12));
        this.subject.ApplyFeedDetails([FeedCheque, FeedVisa], Connected);

        this.subject.FromDate = new DateOnly(2024, 11, 19);

        this.subject.ValidationMessage.ShouldStartWith("The bank feed only has transactions from");
        this.subject.CanExecuteOkButton.ShouldBeFalse();
    }

    [Fact]
    public void Validation_ShouldBlock_GivenNoAccountsTicked()
    {
        PrepareWithLastTransaction(new DateOnly(2026, 11, 12));

        foreach (var row in this.subject.AccountRows)
        {
            row.IsSelected = false;
        }

        this.subject.ValidationMessage.ShouldBe("Tick at least one account.");
        this.subject.CanExecuteOkButton.ShouldBeFalse();
    }

    [Fact]
    public void Validation_ShouldBlock_GivenToIsToday()
    {
        PrepareWithLastTransaction(new DateOnly(2026, 11, 12));

        this.subject.ToDate = Today;

        this.subject.ValidationMessage.ShouldStartWith("To must be before today");
        this.subject.CanExecuteOkButton.ShouldBeFalse();
    }

    [Fact]
    public void Validation_ShouldSayUpToDate_GivenSuggestedRangeIsEmpty()
    {
        PrepareWithLastTransaction(new DateOnly(2026, 11, 19));

        this.subject.ValidationMessage.ShouldStartWith("You're up to date.");
        this.subject.CanExecuteOkButton.ShouldBeFalse();
    }

    private static TransactionsListModel CreateTransactions(DateOnly lastTransactionDate)
    {
        return new TransactionsListModel(new FakeLogger()).LoadTransactions([
            new Transaction { Account = Cheque, Date = lastTransactionDate.AddDays(-3), Amount = -1M, Description = "TEST 1" },
            new Transaction { Account = Cheque, Date = lastTransactionDate, Amount = -2M, Description = "TEST 2" },
            new Transaction { Account = Visa, Date = lastTransactionDate.AddDays(-5), Amount = -3M, Description = "TEST 3" }
        ]);
    }

    private static async Task WaitUntil(Func<bool> condition)
    {
        for (var i = 0; i < 100 && !condition(); i++)
        {
            await Task.Delay(10);
        }

        condition().ShouldBeTrue();
    }

    private void PrepareWithLastTransaction(DateOnly lastTransactionDate)
    {
        this.subject.Prepare(CreateTransactions(lastTransactionDate), Today);
    }
}
