using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.BankFeed;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Settings;
using CommunityToolkit.Mvvm.Messaging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Rees.Wpf.Contracts;
using Shouldly;

namespace BudgetAnalyser.Wpf.XUnit3.Settings;

public class BankFeedSettingsControllerTest
{
    private static readonly ChequeAccount Cheque = new(AccountTypeRepositoryConstants.Cheque);
    private static readonly BankFeedAccount FeedCheque = new("acc_test_1", "Test Bank A", "Test Everyday", "00-0000-0000000-01", true, DateTimeOffset.Parse("2026-11-09T18:00:00Z"));
    private static readonly BankFeedAccount FeedVisa = new("acc_test_2", "Test Bank B", "Test Card", null, true, null);

    [Fact]
    public async Task Apply_ShouldSaveMappingsFromRows()
    {
        var context = CreateContext(budgetFileOpen: true);
        await context.Subject.RefreshAccountsCommand.ExecuteAsync(null);
        var visaRow = context.Subject.AccountRows.Single(r => r.ExternalAccountId == "acc_test_2");
        visaRow.SelectedAccount = visaRow.AccountChoices.Single(c => c.Account?.Name == AccountTypeRepositoryConstants.Visa);
        visaRow.Reference1Source = BankFeedSourceField.Particulars;
        visaRow.AddTypeName();
        visaRow.TypeNames[0].FeedType = "EFTPOS";
        visaRow.TypeNames[0].ImportAs = "EFTPOS TRANSACTION";

        context.Subject.ApplyMappingsCommand.Execute(null);

        context.Configuration.Received(1).SaveMappings(Arg.Is<IEnumerable<BankFeedAccountMapping>>(m => m != null &&
            m.Count() == 2
            && m.Any(x => x.ExternalAccountId == "acc_test_1" && x.Account.Name == AccountTypeRepositoryConstants.Cheque)
            && m.Any(x => x.ExternalAccountId == "acc_test_2"
                          && x.Account.Name == AccountTypeRepositoryConstants.Visa
                          && x.FieldMapping.Reference1.Single() == BankFeedSourceField.Particulars
                          && x.FieldMapping.TypeNames["EFTPOS"] == "EFTPOS TRANSACTION")));
    }

    [Fact]
    public void Apply_ShouldShowProblem_GivenDuplicateTypeName()
    {
        var context = CreateContext(budgetFileOpen: true);
        var row = context.Subject.AccountRows.Single();
        row.AddTypeName();
        row.AddTypeName();
        row.TypeNames[0].FeedType = "EFTPOS";
        row.TypeNames[1].FeedType = "EFTPOS";

        context.Subject.ApplyMappingsCommand.Execute(null);

        context.MessageBox.Received(1).Show(Arg.Is<string>(s => s != null && s.Contains("EFTPOS")), Arg.Any<string>());
        context.Configuration.DidNotReceive().SaveMappings(Arg.Any<IEnumerable<BankFeedAccountMapping>>());
    }

    [Fact]
    public void AccountsCard_ShouldBeDisabled_GivenNoBudgetFileOpen()
    {
        var context = CreateContext(budgetFileOpen: false);

        context.Subject.IsBudgetFileOpen.ShouldBeFalse();
        context.Subject.AccountRows.ShouldBeEmpty();
        context.Subject.RefreshAccountsCommand.CanExecute(null).ShouldBeFalse();
        context.Subject.ApplyMappingsCommand.CanExecute(null).ShouldBeFalse();
    }

    [Fact]
    public void ClearCredentials_ShouldClear_GivenUserConfirms()
    {
        var context = CreateContext();
        context.YesNoBox.Show(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        context.Subject.ClearCredentialsCommand.Execute(null);

        context.Configuration.Received(1).ClearCredentials();
    }

    [Fact]
    public void ClearCredentials_ShouldNotClear_GivenUserDeclines()
    {
        var context = CreateContext();
        context.YesNoBox.Show(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        context.Subject.ClearCredentialsCommand.Execute(null);

        context.Configuration.DidNotReceive().ClearCredentials();
    }

    [Fact]
    public void ConfigurationChanged_ShouldKeepUnappliedEdits_GivenOnlyCredentialsChanged()
    {
        var context = CreateContext(budgetFileOpen: true);
        var row = context.Subject.AccountRows.Single();
        row.Reference2Source = BankFeedSourceField.Code;

        context.Configuration.ConfigurationChanged += Raise.Event();

        context.Subject.AccountRows.Single().ShouldBeSameAs(row);
        context.Subject.HasUnappliedChanges.ShouldBeTrue();
    }

    [Fact]
    public void ConfigurationChanged_ShouldRebuildRows_GivenBudgetFileClosed()
    {
        var context = CreateContext(budgetFileOpen: true);
        context.Configuration.IsBudgetFileOpen.Returns(false);
        context.Configuration.Mappings.Returns([]);

        context.Configuration.ConfigurationChanged += Raise.Event();

        context.Subject.AccountRows.ShouldBeEmpty();
        context.Subject.IsBudgetFileOpen.ShouldBeFalse();
    }

    [Fact]
    public void CredentialRows_ShouldComeFromProviderFields()
    {
        var context = CreateContext();

        context.Subject.CredentialRows.Count.ShouldBe(2);
        context.Subject.CredentialRows[0].Label.ShouldBe("Token 1");
        context.Subject.CredentialRows[0].IsSecret.ShouldBeTrue();
        context.Subject.CredentialRows[0].HasStoredValue.ShouldBeTrue();
        context.Subject.CredentialRows[0].NewValue.ShouldBeEmpty();
    }

    [Fact]
    public void EditingARow_ShouldEnableApplyAndRevert()
    {
        var context = CreateContext(budgetFileOpen: true);

        context.Subject.AccountRows.Single().DescriptionFallback = BankFeedSourceField.MerchantName;

        context.Subject.HasUnappliedChanges.ShouldBeTrue();
        context.Subject.ApplyMappingsCommand.CanExecute(null).ShouldBeTrue();
        context.Subject.RevertMappingsCommand.CanExecute(null).ShouldBeTrue();
    }

    [Fact]
    public void ExistingMapping_ShouldShowDetailsNotLoaded_BeforeRefresh()
    {
        var context = CreateContext(budgetFileOpen: true);

        var row = context.Subject.AccountRows.Single();
        row.FoundInFeed.ShouldBeNull();
        row.Title.ShouldBe("acc_test_1");
        row.SelectedAccount.Account.ShouldBe(Cheque);
    }

    [Fact]
    public async Task Preview_ShouldShowTransactions_WithoutImporting()
    {
        var context = CreateContext(budgetFileOpen: true);
        var preview = new Transaction { Account = Cheque, Description = "TEST PREVIEW", Date = new DateOnly(2026, 11, 9) };
        context.Preview.PreviewAsync(Arg.Any<BankFeedAccountMapping>(), Arg.Any<DateOnly>(), BankFeedSettingsController.PreviewCount, Arg.Any<CancellationToken>()).Returns([preview]);
        var row = context.Subject.AccountRows.Single();

        await context.Subject.PreviewCommand.ExecuteAsync(row);

        row.PreviewTransactions.ShouldBe([preview]);
        row.HasPreviewTransactions.ShouldBeTrue();
        context.Configuration.DidNotReceive().SaveMappings(Arg.Any<IEnumerable<BankFeedAccountMapping>>());
    }

    [Fact]
    public async Task Preview_ShouldUseUnappliedFieldMapping()
    {
        var context = CreateContext(budgetFileOpen: true);
        context.Preview.PreviewAsync(Arg.Any<BankFeedAccountMapping>(), Arg.Any<DateOnly>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns([]);
        var row = context.Subject.AccountRows.Single();
        row.DescriptionSource = BankFeedSourceField.MerchantName;

        await context.Subject.PreviewCommand.ExecuteAsync(row);

        await context.Preview.Received(1).PreviewAsync(
            Arg.Is<BankFeedAccountMapping>(m => m != null && m.FieldMapping.Description.Single() == BankFeedSourceField.MerchantName),
            Arg.Any<DateOnly>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
        row.PreviewMessage.ShouldContain("No transactions");
    }

    [Fact]
    public void PreviewCommand_ShouldBeDisabled_GivenRowNotMapped()
    {
        var context = CreateContext(budgetFileOpen: true);
        var row = context.Subject.AccountRows.Single();

        row.SelectedAccount = row.AccountChoices[0];

        context.Subject.PreviewCommand.CanExecute(row).ShouldBeFalse();
    }

    [Fact]
    public async Task Refresh_ShouldAddUnmappedFeedAccounts_AsDontImport()
    {
        var context = CreateContext(budgetFileOpen: true);

        await context.Subject.RefreshAccountsCommand.ExecuteAsync(null);

        context.Subject.AccountRows.Count.ShouldBe(2);
        var visaRow = context.Subject.AccountRows[1];
        visaRow.Title.ShouldBe("Test Bank B  Test Card");
        visaRow.IsMapped.ShouldBeFalse();
        visaRow.DescriptionSource.ShouldBe(BankFeedSourceField.Description);
        visaRow.TypeSource.ShouldBe(BankFeedSourceField.Type);
    }

    [Fact]
    public async Task Refresh_ShouldFlagMappingNotFoundInFeed_AndAllowRemove()
    {
        var context = CreateContext(budgetFileOpen: true);
        context.Configuration.GetFeedAccountsAsync(Arg.Any<CancellationToken>()).Returns([FeedVisa]);

        await context.Subject.RefreshAccountsCommand.ExecuteAsync(null);
        var missing = context.Subject.AccountRows.Single(r => r.ExternalAccountId == "acc_test_1");

        missing.IsNotFoundInFeed.ShouldBeTrue();
        missing.StatusText.ShouldBe("Account not found in bank feed.");
        context.Subject.RemoveMappingCommand.Execute(missing);
        context.Subject.AccountRows.ShouldNotContain(missing);
        context.Subject.HasUnappliedChanges.ShouldBeTrue();
    }

    [Fact]
    public async Task Refresh_ShouldShowRejectedMessage_GivenCredentialsRejected()
    {
        var context = CreateContext(budgetFileOpen: true);
        context.Configuration.GetFeedAccountsAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new BankFeedAuthenticationException("Rejected"));

        await context.Subject.RefreshAccountsCommand.ExecuteAsync(null);

        context.Subject.MappingStatus.ShouldContain("rejected");
        context.Subject.IsBusy.ShouldBeFalse();
    }

    [Fact]
    public void Revert_ShouldDiscardUnappliedEdits()
    {
        var context = CreateContext(budgetFileOpen: true);
        context.Subject.AccountRows.Single().Reference1Source = BankFeedSourceField.Code;

        context.Subject.RevertMappingsCommand.Execute(null);

        context.Subject.HasUnappliedChanges.ShouldBeFalse();
        context.Subject.AccountRows.Single().Reference1Source.ShouldBe(BankFeedSourceField.None);
    }

    [Fact]
    public void SaveCredentials_ShouldRequireEveryField()
    {
        var context = CreateContext();

        context.Subject.CredentialRows[0].NewValue = "test-token-1";

        context.Subject.SaveCredentialsCommand.CanExecute(null).ShouldBeFalse();
    }

    [Fact]
    public void SaveCredentials_ShouldSaveAllEnteredValues()
    {
        var context = CreateContext();
        context.Subject.CredentialRows[0].NewValue = " test-token-1 ";
        context.Subject.CredentialRows[1].NewValue = "test-token-2";

        context.Subject.SaveCredentialsCommand.Execute(null);

        context.Configuration.Received(1).SaveCredentials(Arg.Is<IReadOnlyDictionary<string, string>>(v => v != null && v["Token1"] == "test-token-1" && v["Token2"] == "test-token-2"));
        context.Subject.ConnectionStatus.ShouldContain("Test connection");
    }

    [Fact]
    public async Task TestConnection_ShouldShowConnectedAndLoadAccounts()
    {
        var context = CreateContext(budgetFileOpen: true);

        await context.Subject.TestConnectionCommand.ExecuteAsync(null);

        context.Subject.ConnectionStatus.ShouldBe("Connected as test.user@example.com.");
        await context.Configuration.Received(1).GetFeedAccountsAsync(Arg.Any<CancellationToken>());
        context.Subject.AccountRows.Single(r => r.ExternalAccountId == "acc_test_1").FoundInFeed.ShouldBe(true);
    }

    [Fact]
    public async Task TestConnection_ShouldShowRejectedMessage_GivenCredentialsRejected()
    {
        var context = CreateContext();
        context.Configuration.TestConnectionAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new BankFeedAuthenticationException("Rejected"));

        await context.Subject.TestConnectionCommand.ExecuteAsync(null);

        context.Subject.ConnectionStatus.ShouldContain("rejected");
        await context.Configuration.DidNotReceive().GetFeedAccountsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void TestConnection_ShouldBeDisabled_GivenCredentialsNotSaved()
    {
        var context = CreateContext(credentialsSaved: false);

        context.Subject.TestConnectionCommand.CanExecute(null).ShouldBeFalse();
    }

    internal static Context CreateContext(bool budgetFileOpen = false, bool credentialsSaved = true)
    {
        var configuration = Substitute.For<IBankFeedConfigurationService>();
        configuration.ProviderDisplayName.Returns("Test Provider");
        configuration.CredentialFields.Returns([new BankFeedCredentialField("Token1", "Token 1", true), new BankFeedCredentialField("Token2", "Token 2", true)]);
        configuration.HasStoredValue(Arg.Any<string>()).Returns(credentialsSaved);
        configuration.IsBudgetFileOpen.Returns(budgetFileOpen);
        IReadOnlyList<BankFeedAccountMapping> mappings = budgetFileOpen ? [new BankFeedAccountMapping("acc_test_1", Cheque, BankFeedFieldMapping.Default)] : [];
        configuration.Mappings.Returns(mappings);
        configuration.TestConnectionAsync(Arg.Any<CancellationToken>()).Returns(new BankFeedConnectionStatus(true, "test.user@example.com", new DateOnly(2024, 11, 10)));
        configuration.GetFeedAccountsAsync(Arg.Any<CancellationToken>()).Returns([FeedCheque, FeedVisa]);

        var accounts = Substitute.For<IAccountTypeRepository>();
        accounts.ListCurrentlyUsedAccountTypes().Returns([Cheque, new VisaAccount(AccountTypeRepositoryConstants.Visa)]);

        var messageBox = Substitute.For<IUserMessageBox>();
        var yesNoBox = Substitute.For<IUserQuestionBoxYesNo>();
        var userPrompts = new UserPrompts(messageBox, () => Substitute.For<IUserPromptOpenFile>(), () => Substitute.For<IUserPromptSaveFile>(), yesNoBox, Substitute.For<IUserInputBox>());
        var preview = Substitute.For<IBankFeedPreviewService>();

        var subject = new BankFeedSettingsController(new WeakReferenceMessenger(), configuration, preview, accounts, userPrompts, Substitute.For<ILogger>());
        return new Context(subject, configuration, preview, messageBox, yesNoBox);
    }

    internal sealed record Context(
        BankFeedSettingsController Subject,
        IBankFeedConfigurationService Configuration,
        IBankFeedPreviewService Preview,
        IUserMessageBox MessageBox,
        IUserQuestionBoxYesNo YesNoBox);
}
