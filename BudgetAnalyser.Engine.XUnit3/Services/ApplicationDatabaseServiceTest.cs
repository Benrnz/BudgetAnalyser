using System.Security;
using System.Text;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using Rees.UnitTestUtilities;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Services;

public class ApplicationDatabaseServiceTest
{
    private readonly ICredentialStore mockCredentials;
    private readonly IDirtyDataService mockDirtyService;
    private readonly IApplicationDatabaseRepository mockRepo;
    private readonly ISupportsModelPersistence mockService1;
    private readonly ISupportsModelPersistence mockService2;
    private readonly IEnumerable<ISupportsModelPersistence> mockServices;
    private readonly ApplicationDatabaseService subject;

    public ApplicationDatabaseServiceTest()
    {
        this.mockRepo = Substitute.For<IApplicationDatabaseRepository>();
        this.mockService1 = Substitute.For<ISupportsModelPersistence>();
        this.mockService1.LoadSequence.Returns(50);

        this.mockService2 = Substitute.For<ISupportsModelPersistence>();
        this.mockService2.LoadSequence.Returns(20);

        this.mockCredentials = Substitute.For<ICredentialStore>();
        this.mockServices = new[] { this.mockService1, this.mockService2 };
        this.mockDirtyService = Substitute.For<IDirtyDataService>();

        this.subject = new ApplicationDatabaseService(
            this.mockRepo,
            this.mockServices,
            new FakeMonitorableDependencies(),
            this.mockCredentials,
            new FakeLogger(),
            this.mockDirtyService);
    }

    [Fact]
    public void Close_ShouldCloseAllServices()
    {
        PrivateAccessor.SetField(this.subject, "budgetAnalyserDatabase", new ApplicationDatabase());

        this.subject.Close();

        this.mockService1.Received(1).Close();
        this.mockService2.Received(1).Close();
    }

    [Fact]
    public async Task Close_ShouldResetHasUnsavedChangesFlagToFalse()
    {
        this.mockRepo.CreateNewAsync(Arg.Any<string>()).Returns(Task.FromResult(new ApplicationDatabase()));
        var index = 0;
        this.mockDirtyService.HasUnsavedChanges.Returns(_ => index++ == 0);

        await this.subject.CreateNewDatabaseAsync("Foo");
        this.subject.NotifyOfChange(ApplicationDataType.Budget);
        this.subject.HasUnsavedChanges.ShouldBeTrue();
        this.subject.Close();

        this.subject.HasUnsavedChanges.ShouldBeFalse();
    }

    [Fact]
    public async Task CreateNewDatabaseAsync_ShouldCallCreateOnEachService()
    {
        CreateNewDatabaseSetup();

        await this.subject.CreateNewDatabaseAsync("Foo");

        await this.mockService1.Received(1).CreateNewAsync(Arg.Any<ApplicationDatabase>());
        await this.mockService2.Received(1).CreateNewAsync(Arg.Any<ApplicationDatabase>());
    }

    [Fact]
    public async Task CreateNewDatabaseAsync_ShouldReturnNonNullAppDb()
    {
        CreateNewDatabaseSetup();

        var appDb = await this.subject.CreateNewDatabaseAsync("Foo");

        appDb.ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateNewDatabaseAsync_ShouldThrow_GivenEmptyStorageKey()
    {
        await Should.ThrowAsync<ArgumentNullException>(async () => await this.subject.CreateNewDatabaseAsync(string.Empty));
    }

    [Fact]
    public async Task CreateNewDatabaseAsync_ShouldThrow_GivenNullStorageKey()
    {
        await Should.ThrowAsync<ArgumentNullException>(async () => await this.subject.CreateNewDatabaseAsync(null!));
    }

    [Fact]
    public void Ctor_ShouldOrderServices()
    {
        var services = PrivateAccessor.GetField(this.subject, "persistenceModelServices") as IEnumerable<ISupportsModelPersistence>;
        services.ShouldNotBeNull();
        var sequence = 0;
        foreach (var service in services)
        {
            (sequence <= service.LoadSequence).ShouldBeTrue(
                "Services are not ordered in ascending order. This is important to ensure critical and root dependent services are evaluated first.");
            sequence = service.LoadSequence;
        }
    }

    [Fact]
    public void Ctor_ShouldThrow_GivenNullRepo()
    {
        Should.Throw<ArgumentNullException>(() =>
            new ApplicationDatabaseService(
                null!,
                this.mockServices,
                new FakeMonitorableDependencies(),
                this.mockCredentials,
                new FakeLogger(),
                Substitute.For<IDirtyDataService>()));
    }

    [Fact]
    public void Ctor_ShouldThrow_GivenNullServices()
    {
        Should.Throw<ArgumentNullException>(() =>
            new ApplicationDatabaseService(
                this.mockRepo,
                null!,
                new FakeMonitorableDependencies(),
                this.mockCredentials,
                new FakeLogger(),
                Substitute.For<IDirtyDataService>()));
    }

    [Fact]
    public async Task EncryptFilesAsync_ShouldThrow_GivenNoClaimSet()
    {
        var appDb = new ApplicationDatabase { FileName = "Foo.bax" };
        PrivateAccessor.SetField(this.subject, "budgetAnalyserDatabase", appDb);

        await Should.ThrowAsync<EncryptionKeyNotProvidedException>(async () => await this.subject.EncryptFilesAsync());
    }

    [Fact]
    public async Task EncryptFilesAsync_ShouldThrow_GivenNoDatabaseFile()
    {
        await Should.ThrowAsync<ArgumentException>(async () => await this.subject.EncryptFilesAsync());
    }

    [Fact]
    public void InitialState_ShouldHaveNoUnsavedChanges()
    {
        this.subject.HasUnsavedChanges.ShouldBeFalse();
    }

    [Fact]
    public async Task LoadDatabaseAsync_ShouldCallCreateOnEachService()
    {
        LoadDatabaseSetup();

        await this.subject.LoadAsync("Foo");

        await this.mockService1.Received(1).LoadAsync(Arg.Any<ApplicationDatabase>());
        await this.mockService2.Received(1).LoadAsync(Arg.Any<ApplicationDatabase>());
    }

    [Fact]
    public async Task LoadDatabaseAsync_ShouldResetHasUnSavedChanges()
    {
        LoadDatabaseSetup();

        await this.subject.LoadAsync("Foo");

        this.subject.HasUnsavedChanges.ShouldBeFalse();
    }

    [Fact]
    public async Task LoadDatabaseAsync_ShouldReturnNonNullAppDb()
    {
        LoadDatabaseSetup();

        var appDb = await this.subject.LoadAsync("Foo");

        appDb.ShouldNotBeNull();
    }

    [Fact]
    public async Task LoadDatabaseAsync_ShouldThrow_GivenEmptyStorageKey()
    {
        await Should.ThrowAsync<ArgumentNullException>(async () => await this.subject.LoadAsync(string.Empty));
    }

    [Fact]
    public async Task LoadDatabaseAsync_ShouldThrow_GivenNullStorageKey()
    {
        await Should.ThrowAsync<ArgumentNullException>(async () => await this.subject.LoadAsync(null!));
    }

    [Fact]
    public async Task LoadDatabaseAsync_ShouldThrow_WhenServiceThrowsDataFormatException()
    {
        LoadDatabaseSetup();
        this.mockService1.LoadAsync(Arg.Any<ApplicationDatabase>())
            .Returns(_ => Task.FromException(new DataFormatException("bad")));

        await Should.ThrowAsync<DataFormatException>(async () => await this.subject.LoadAsync("Foo"));
    }

    [Fact]
    public async Task LoadDatabaseAsync_ShouldThrow_WhenServiceThrowsKeyNotFoundException()
    {
        LoadDatabaseSetup();
        this.mockService1.LoadAsync(Arg.Any<ApplicationDatabase>())
            .Returns(_ => Task.FromException(new KeyNotFoundException("missing")));

        await Should.ThrowAsync<KeyNotFoundException>(async () => await this.subject.LoadAsync("Foo"));
    }

    [Fact]
    public async Task LoadDatabaseAsync_ShouldThrow_WhenServiceThrowsNotSupportedException()
    {
        LoadDatabaseSetup();
        this.mockService1.LoadAsync(Arg.Any<ApplicationDatabase>())
            .Returns(_ => Task.FromException(new NotSupportedException("unsupported")));

        await Should.ThrowAsync<DataFormatException>(async () => await this.subject.LoadAsync("Foo"));
    }

    [Fact]
    public async Task Save_ShouldCallSaveOnAllServices()
    {
        SaveSetup();
        await this.subject.SaveAsync();

        await this.mockService1.Received(1).SaveAsync(Arg.Any<ApplicationDatabase>());
        await this.mockService2.Received(1).SaveAsync(Arg.Any<ApplicationDatabase>());
    }

    [Fact]
    public async Task Save_ShouldCallSaveOnRepository()
    {
        SaveSetup();
        await this.subject.SaveAsync();

        await this.mockRepo.Received(1).SaveAsync(Arg.Any<ApplicationDatabase>());
    }

    [Fact]
    public async Task Save_ShouldThrow_GivenNothingLoaded()
    {
        await Should.ThrowAsync<InvalidOperationException>(async () => await this.subject.SaveAsync());
    }

    [Fact]
    public void SetClaim_ShouldCallCredentialStore_GivenValidClaim()
    {
        this.subject.SetCredential(CreateSecureString("Foo"));

        this.mockCredentials.Received(1).SetPasskey(Arg.Any<SecureString>());
    }

    private void CreateNewDatabaseSetup()
    {
        this.mockRepo.CreateNewAsync(Arg.Any<string>())
            .Returns(Task.FromResult(new ApplicationDatabase()));
    }

    private static SecureString CreateSecureString(string text)
    {
        var securedText = new SecureString();
        foreach (var c in text)
        {
            securedText.AppendChar(c);
        }

        return securedText;
    }

    private void LoadDatabaseSetup()
    {
        this.mockRepo.LoadAsync(Arg.Any<string>())
            .Returns(Task.FromResult(new ApplicationDatabase()));
    }

    private void SaveSetup()
    {
        PrivateAccessor.SetField(this.subject, "budgetAnalyserDatabase", new ApplicationDatabase());
        this.mockService1.ValidateModel(Arg.Any<StringBuilder>()).Returns(true);
        this.mockService2.ValidateModel(Arg.Any<StringBuilder>()).Returns(true);
        this.mockDirtyService.HasUnsavedChanges.Returns(true);
        this.mockDirtyService.IsDirty(ApplicationDataType.Budget).Returns(true);
        this.subject.NotifyOfChange(ApplicationDataType.Budget);
    }
}
