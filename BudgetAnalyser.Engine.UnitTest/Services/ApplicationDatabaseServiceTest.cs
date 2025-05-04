using System.Security;
using System.Text;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.UnitTest.Encryption;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Moq;
using Rees.UnitTestUtilities;

namespace BudgetAnalyser.Engine.UnitTest.Services;

[TestClass]
public class ApplicationDatabaseServiceTest
{
    private Mock<ICredentialStore> mockCredentials;
    private Mock<IDirtyDataService> mockDirtyService;
    private Mock<IApplicationDatabaseRepository> mockRepo;
    private Mock<ISupportsModelPersistence> mockService1;
    private Mock<ISupportsModelPersistence> mockService2;
    private IEnumerable<ISupportsModelPersistence> mockServices;
    private ApplicationDatabaseService subject;

    [TestMethod]
    public void Close_ShouldCloseAllServices()
    {
        PrivateAccessor.SetField(this.subject, "budgetAnalyserDatabase", new Mock<ApplicationDatabase>().Object);

        this.subject.Close();

        this.mockService1.Verify(m => m.Close());
        this.mockService2.Verify(m => m.Close());
    }

    [TestMethod]
    public async Task Close_ShouldResetHasUnsavedChangesFlagToFalse()
    {
        this.mockRepo.Setup(m => m.CreateNewAsync(It.IsAny<string>()))
            .Returns(Task.FromResult(new ApplicationDatabase()));
        var index = 0;
        this.mockDirtyService.Setup(m => m.HasUnsavedChanges).Returns(() => index++ == 0);

        await this.subject.CreateNewDatabaseAsync("Foo");
        this.subject.NotifyOfChange(ApplicationDataType.Budget);
        Assert.IsTrue(this.subject.HasUnsavedChanges);
        this.subject.Close();

        Assert.IsFalse(this.subject.HasUnsavedChanges);
    }

    [TestMethod]
    public async Task CreateNewDatabaseAsync_ShouldCallCreateOnEachService()
    {
        CreateNewDatabaseSetup();

        var appDb = await this.subject.CreateNewDatabaseAsync("Foo");

        this.mockService1.Verify(m => m.CreateNewAsync(It.IsAny<ApplicationDatabase>()));
        this.mockService2.Verify(m => m.CreateNewAsync(It.IsAny<ApplicationDatabase>()));
    }

    [TestMethod]
    public async Task CreateNewDatabaseAsync_ShouldReturnNonNullAppDb()
    {
        CreateNewDatabaseSetup();

        var appDb = await this.subject.CreateNewDatabaseAsync("Foo");

        Assert.IsNotNull(appDb);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task CreateNewDatabaseAsync_ShouldThrow_GivenEmptyStorageKey()
    {
        await this.subject.CreateNewDatabaseAsync(string.Empty);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task CreateNewDatabaseAsync_ShouldThrow_GivenNullStorageKey()
    {
        await this.subject.CreateNewDatabaseAsync(null);
    }

    [TestMethod]
    public void Ctor_ShouldOrderServices()
    {
        var services = (IEnumerable<ISupportsModelPersistence>)PrivateAccessor.GetField(this.subject, "persistenceModelServices");
        var sequence = 0;
        foreach (var service in services)
        {
            if (sequence > service.LoadSequence)
            {
                Assert.Fail("Services are not ordered in ascending order. This is important to ensure critical and root dependent services are evaluated first.");
            }

            sequence = service.LoadSequence;
        }
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Ctor_ShouldThrow_GivenNullRepo()
    {
        new ApplicationDatabaseService(null, this.mockServices, new FakeMonitorableDependencies(), this.mockCredentials.Object, new FakeLogger(), new Mock<IDirtyDataService>().Object);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Ctor_ShouldThrow_GivenNullServices()
    {
        new ApplicationDatabaseService(this.mockRepo.Object, null, new FakeMonitorableDependencies(), this.mockCredentials.Object, new FakeLogger(), new Mock<IDirtyDataService>().Object);
    }

    [TestMethod]
    [ExpectedException(typeof(EncryptionKeyNotProvidedException))]
    public async Task EncryptFilesAsync_ShouldThrow_GivenNoClaimSet()
    {
        var appDb = new ApplicationDatabase { FileName = "Foo.bax" };
        PrivateAccessor.SetField(this.subject, "budgetAnalyserDatabase", appDb);
        await this.subject.EncryptFilesAsync();

        Assert.Fail();
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task EncryptFilesAsync_ShouldThrow_GivenNoDatabaseFile()
    {
        await this.subject.EncryptFilesAsync();

        Assert.Fail();
    }

    [TestMethod]
    public void InitialState_ShouldHaveNoUnsavedChanges()
    {
        Assert.IsFalse(this.subject.HasUnsavedChanges);
    }

    [TestMethod]
    public async Task LoadDatabaseAsync_ShouldCallCreateOnEachService()
    {
        LoadDatabaseSetup();

        var appDb = await this.subject.LoadAsync("Foo");

        this.mockService1.Verify(m => m.LoadAsync(It.IsAny<ApplicationDatabase>()));
        this.mockService2.Verify(m => m.LoadAsync(It.IsAny<ApplicationDatabase>()));
    }

    [TestMethod]
    public async Task LoadDatabaseAsync_ShouldResetHasUnSavedChanges()
    {
        LoadDatabaseSetup();

        var appDb = await this.subject.LoadAsync("Foo");

        Assert.IsFalse(this.subject.HasUnsavedChanges);
    }

    [TestMethod]
    public async Task LoadDatabaseAsync_ShouldReturnNonNullAppDb()
    {
        LoadDatabaseSetup();

        var appDb = await this.subject.LoadAsync("Foo");

        Assert.IsNotNull(appDb);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task LoadDatabaseAsync_ShouldThrow_GivenEmptyStorageKey()
    {
        await this.subject.LoadAsync(string.Empty);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task LoadDatabaseAsync_ShouldThrow_GivenNullStorageKey()
    {
        await this.subject.LoadAsync(null);
    }

    [TestMethod]
    [ExpectedException(typeof(DataFormatException))]
    public async Task LoadDatabaseAsync_ShouldThrow_WhenServiceThrowsDataFormatException()
    {
        LoadDatabaseSetup();

        this.mockService1.Setup(m => m.LoadAsync(It.IsAny<ApplicationDatabase>())).Throws<DataFormatException>();

        var appDb = await this.subject.LoadAsync("Foo");
    }

    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public async Task LoadDatabaseAsync_ShouldThrow_WhenServiceThrowsKeyNotFoundException()
    {
        LoadDatabaseSetup();

        this.mockService1.Setup(m => m.LoadAsync(It.IsAny<ApplicationDatabase>())).Throws<KeyNotFoundException>();

        var appDb = await this.subject.LoadAsync("Foo");
    }

    [TestMethod]
    [ExpectedException(typeof(DataFormatException))]
    public async Task LoadDatabaseAsync_ShouldThrow_WhenServiceThrowsNotSupportedException()
    {
        LoadDatabaseSetup();

        this.mockService1.Setup(m => m.LoadAsync(It.IsAny<ApplicationDatabase>())).Throws<NotSupportedException>();

        var appDb = await this.subject.LoadAsync("Foo");
    }

    [TestMethod]
    public async Task Save_ShouldCallSaveOnAllServices()
    {
        SaveSetup();
        await this.subject.SaveAsync();

        this.mockService1.Verify(m => m.SaveAsync(It.IsAny<ApplicationDatabase>()));
        this.mockService2.Verify(m => m.SaveAsync(It.IsAny<ApplicationDatabase>()));
    }

    [TestMethod]
    public async Task Save_ShouldCallSaveOnRepository()
    {
        SaveSetup();
        await this.subject.SaveAsync();

        this.mockRepo.Verify(m => m.SaveAsync(It.IsAny<ApplicationDatabase>()));
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task Save_ShouldThrow_GivenNothingLoaded()
    {
        await this.subject.SaveAsync();
    }

    [TestMethod]
    public void SetClaim_ShouldCallCredentialStore_GivenValidClaim()
    {
        this.subject.SetCredential(CredentialStoreTest.CreateSecureString("Foo"));

        this.mockCredentials.Verify(m => m.SetPasskey(It.IsAny<SecureString>()));
    }

    [TestInitialize]
    public void TestInitialise()
    {
        this.mockRepo = new Mock<IApplicationDatabaseRepository>();
        this.mockService1 = new Mock<ISupportsModelPersistence>();
        this.mockService1.Setup(m => m.LoadSequence).Returns(50);

        this.mockService2 = new Mock<ISupportsModelPersistence>();
        this.mockService2.Setup(m => m.LoadSequence).Returns(20);

        this.mockCredentials = new Mock<ICredentialStore>();

        this.mockServices = new[] { this.mockService1.Object, this.mockService2.Object };
        this.mockDirtyService = new Mock<IDirtyDataService>();
        this.subject = new ApplicationDatabaseService(
            this.mockRepo.Object,
            this.mockServices,
            new FakeMonitorableDependencies(),
            this.mockCredentials.Object,
            new FakeLogger(),
            this.mockDirtyService.Object);
    }

    private void CreateNewDatabaseSetup()
    {
        this.mockRepo.Setup(m => m.CreateNewAsync(It.IsAny<string>()))
            .Returns(Task.FromResult(new ApplicationDatabase()));
    }

    private void LoadDatabaseSetup()
    {
        this.mockRepo.Setup(m => m.LoadAsync(It.IsAny<string>()))
            .Returns(Task.FromResult(new ApplicationDatabase()));
    }

    private void SaveSetup()
    {
        PrivateAccessor.SetField(this.subject, "budgetAnalyserDatabase", new Mock<ApplicationDatabase>().Object);
        this.mockService1.Setup(m => m.ValidateModel(It.IsAny<StringBuilder>())).Returns(true);
        this.mockService2.Setup(m => m.ValidateModel(It.IsAny<StringBuilder>())).Returns(true);
        this.mockDirtyService.Setup(m => m.HasUnsavedChanges).Returns(true);
        this.mockDirtyService.Setup(m => m.IsDirty(ApplicationDataType.Budget)).Returns(true);
        this.subject.NotifyOfChange(ApplicationDataType.Budget);
    }
}
