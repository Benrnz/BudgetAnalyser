using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.BankFeed;
using BudgetAnalyser.Engine.BankFeed.Data;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.BankFeed;

public class BankFeedConfigurationServiceTest
{
    private const string ProviderKey = "test.provider";

    private readonly FakeBankFeedCredentialStore credentialStore = new();
    private readonly IDirtyDataService dirtyDataService = Substitute.For<IDirtyDataService>();
    private readonly IBankFeedProvider provider = Substitute.For<IBankFeedProvider>();
    private readonly BankFeedConfigurationService subject;
    private int changedCount;

    public BankFeedConfigurationServiceTest()
    {
        this.provider.ProviderKey.Returns(ProviderKey);
        this.provider.DisplayName.Returns("Test Provider");
        this.provider.CredentialFields.Returns([new BankFeedCredentialField("Token1", "Token 1", true), new BankFeedCredentialField("Token2", "Token 2", true)]);
        this.provider.TestConnectionAsync(Arg.Any<CancellationToken>()).Returns(new BankFeedConnectionStatus(true, "test.user@example.com", new DateOnly(2024, 11, 10)));

        this.subject = new BankFeedConfigurationService(this.provider, this.credentialStore, new InMemoryAccountTypeRepository(), this.dirtyDataService, new FakeLogger());
        this.subject.ConfigurationChanged += (_, _) => this.changedCount++;
    }

    [Fact]
    public async Task ClearCredentials_ShouldResetIsConfigured()
    {
        await ArrangeFullyConfigured();

        this.subject.ClearCredentials();

        this.subject.IsConfigured.ShouldBeFalse();
        this.subject.HasStoredValue("Token1").ShouldBeFalse();
    }

    [Fact]
    public async Task Close_ShouldClearMappingsAndRaiseChanged()
    {
        await ArrangeFullyConfigured();
        this.changedCount = 0;

        this.subject.Close();

        this.subject.Mappings.ShouldBeEmpty();
        this.subject.IsBudgetFileOpen.ShouldBeFalse();
        this.subject.IsConfigured.ShouldBeFalse();
        this.changedCount.ShouldBe(1);
    }

    [Fact]
    public async Task GetConnectionStatusAsync_ShouldNotRecordVerifiedOrRaiseChanged_GivenSuccess()
    {
        await LoadDatabaseWithOneMapping();
        SaveBothCredentials();
        this.changedCount = 0;

        var status = await this.subject.GetConnectionStatusAsync(CancellationToken.None);

        status.HistoryAvailableFrom.ShouldBe(new DateOnly(2024, 11, 10));
        this.credentialStore.GetConnectionVerified(ProviderKey).ShouldBeNull();
        this.subject.IsConfigured.ShouldBeFalse();
        this.changedCount.ShouldBe(0);
    }

    [Fact]
    public async Task GetConnectionStatusAsync_ShouldResetVerified_GivenCredentialsRejected()
    {
        await ArrangeFullyConfigured();
        this.provider.TestConnectionAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new BankFeedAuthenticationException("Rejected"));

        await Should.ThrowAsync<BankFeedAuthenticationException>(() => this.subject.GetConnectionStatusAsync(CancellationToken.None));

        this.subject.IsConfigured.ShouldBeFalse();
    }

    [Fact]
    public async Task GetFeedAccountsAsync_ShouldResetVerified_GivenCredentialsRejected()
    {
        await ArrangeFullyConfigured();
        this.provider.GetAccountsAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new BankFeedAuthenticationException("Rejected"));

        await Should.ThrowAsync<BankFeedAuthenticationException>(() => this.subject.GetFeedAccountsAsync(CancellationToken.None));

        this.subject.IsConfigured.ShouldBeFalse();
    }

    [Fact]
    public void HasStoredValue_ShouldNeverExposeTheValue()
    {
        this.subject.SaveCredentials(new Dictionary<string, string> { ["Token1"] = "secret-test-value" });

        this.subject.HasStoredValue("Token1").ShouldBeTrue();
        this.subject.HasStoredValue("Token2").ShouldBeFalse();
    }

    [Fact]
    public async Task IsConfigured_ShouldBeFalse_GivenMissingCredentialField()
    {
        await LoadDatabaseWithOneMapping();
        this.subject.SaveCredentials(new Dictionary<string, string> { ["Token1"] = "test-token-1" });
        this.credentialStore.SetConnectionVerified(ProviderKey, DateTimeOffset.UtcNow);

        this.subject.IsConfigured.ShouldBeFalse();
    }

    [Fact]
    public async Task IsConfigured_ShouldBeFalse_GivenNoMappings()
    {
        await this.subject.LoadAsync(new ApplicationDatabase());
        SaveBothCredentials();
        await this.subject.TestConnectionAsync(CancellationToken.None);

        this.subject.IsConfigured.ShouldBeFalse();
    }

    [Fact]
    public async Task IsConfigured_ShouldBeFalse_GivenNoSuccessfulConnectionTest()
    {
        await LoadDatabaseWithOneMapping();
        SaveBothCredentials();

        this.subject.IsConfigured.ShouldBeFalse();
    }

    [Fact]
    public async Task IsConfigured_ShouldBeFalse_GivenProviderReportsNotConnected()
    {
        await LoadDatabaseWithOneMapping();
        SaveBothCredentials();
        this.provider.TestConnectionAsync(Arg.Any<CancellationToken>()).Returns(new BankFeedConnectionStatus(false, null, null));

        await this.subject.TestConnectionAsync(CancellationToken.None);

        this.subject.IsConfigured.ShouldBeFalse();
    }

    [Fact]
    public void IsConfigured_ShouldBeFalse_GivenNoBudgetFileOpen()
    {
        SaveBothCredentials();
        this.credentialStore.SetConnectionVerified(ProviderKey, DateTimeOffset.UtcNow);

        this.subject.IsConfigured.ShouldBeFalse();
    }

    [Fact]
    public async Task IsConfigured_ShouldBeTrue_GivenCredentialsVerifiedConnectionAndMapping()
    {
        await ArrangeFullyConfigured();

        this.subject.IsConfigured.ShouldBeTrue();
    }

    [Fact]
    public async Task LoadAsync_ShouldDropMappingWithUnknownAccount()
    {
        var database = new ApplicationDatabase
        {
            BankFeedAccountMappings = [new BankFeedAccountMappingDto("acc_test_1", "CHEQUE"), new BankFeedAccountMappingDto("acc_test_2", "NOSUCHACCOUNT")]
        };

        await this.subject.LoadAsync(database);

        this.subject.Mappings.Count.ShouldBe(1);
        this.subject.Mappings[0].Account.Name.ShouldBe(AccountTypeRepositoryConstants.Cheque);
    }

    [Fact]
    public async Task LoadAsync_ShouldRaiseChanged()
    {
        await this.subject.LoadAsync(new ApplicationDatabase());

        this.changedCount.ShouldBe(1);
        this.subject.IsBudgetFileOpen.ShouldBeTrue();
    }

    [Fact]
    public async Task NotifyCredentialsRejected_ShouldResetVerifiedAndRaiseChanged()
    {
        await ArrangeFullyConfigured();
        this.changedCount = 0;

        this.subject.NotifyCredentialsRejected();

        this.subject.IsConfigured.ShouldBeFalse();
        this.changedCount.ShouldBe(1);
    }

    [Fact]
    public async Task SaveCredentials_ShouldResetVerified()
    {
        await ArrangeFullyConfigured();

        SaveBothCredentials();

        this.subject.IsConfigured.ShouldBeFalse();
    }

    [Fact]
    public void SaveCredentials_ShouldThrow_GivenUnknownField()
    {
        Should.Throw<ArgumentException>(() => this.subject.SaveCredentials(new Dictionary<string, string> { ["NotAField"] = "x" }));
    }

    [Fact]
    public async Task SaveMappings_ShouldUpdateDatabaseAndMarkDirty()
    {
        var database = new ApplicationDatabase();
        await this.subject.LoadAsync(database);
        var mapping = new BankFeedAccountMapping("acc_test_9", new SavingsAccount(AccountTypeRepositoryConstants.Savings), BankFeedFieldMapping.Default);

        this.subject.SaveMappings([mapping]);

        this.subject.Mappings.ShouldBe([mapping]);
        database.BankFeedAccountMappings.Single().ExternalAccountId.ShouldBe("acc_test_9");
        database.BankFeedAccountMappings.Single().Account.ShouldBe(AccountTypeRepositoryConstants.Savings);
        this.dirtyDataService.Received(1).NotifyOfChange(ApplicationDataType.BankFeed);
    }

    [Fact]
    public async Task SaveMappings_ShouldThrow_GivenSameFeedAccountMappedTwice()
    {
        await this.subject.LoadAsync(new ApplicationDatabase());
        var mapping = new BankFeedAccountMapping("acc_test_1", new ChequeAccount(AccountTypeRepositoryConstants.Cheque), BankFeedFieldMapping.Default);

        Should.Throw<ArgumentException>(() => this.subject.SaveMappings([mapping, mapping with { Account = new VisaAccount(AccountTypeRepositoryConstants.Visa) }]));
    }

    [Fact]
    public void SaveMappings_ShouldThrow_GivenNoBudgetFileOpen()
    {
        Should.Throw<InvalidOperationException>(() => this.subject.SaveMappings([]));
    }

    [Fact]
    public async Task TestConnectionAsync_ShouldResetVerified_GivenCredentialsRejected()
    {
        await ArrangeFullyConfigured();
        this.provider.TestConnectionAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new BankFeedAuthenticationException("Rejected"));

        await Should.ThrowAsync<BankFeedAuthenticationException>(() => this.subject.TestConnectionAsync(CancellationToken.None));

        this.subject.IsConfigured.ShouldBeFalse();
    }

    [Fact]
    public async Task TestConnectionAsync_ShouldKeepVerified_GivenNetworkError()
    {
        await ArrangeFullyConfigured();
        this.provider.TestConnectionAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new BankFeedException("Network down"));

        await Should.ThrowAsync<BankFeedException>(() => this.subject.TestConnectionAsync(CancellationToken.None));

        this.subject.IsConfigured.ShouldBeTrue();
    }

    private async Task ArrangeFullyConfigured()
    {
        await LoadDatabaseWithOneMapping();
        SaveBothCredentials();
        await this.subject.TestConnectionAsync(CancellationToken.None);
    }

    private async Task LoadDatabaseWithOneMapping()
    {
        await this.subject.LoadAsync(new ApplicationDatabase { BankFeedAccountMappings = [new BankFeedAccountMappingDto("acc_test_1", "CHEQUE")] });
    }

    private void SaveBothCredentials()
    {
        this.subject.SaveCredentials(new Dictionary<string, string> { ["Token1"] = "test-token-1", ["Token2"] = "test-token-2" });
    }
}
