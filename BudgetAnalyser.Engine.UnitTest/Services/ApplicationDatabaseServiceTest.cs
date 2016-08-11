using System;
using System.Collections.Generic;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.UnitTest.Encryption;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rees.UnitTestUtilities;

namespace BudgetAnalyser.Engine.UnitTest.Services
{
    [TestClass]
    public class ApplicationDatabaseServiceTest
    {
        private Mock<ICredentialStore> mockCredentials;
        private Mock<IApplicationDatabaseRepository> mockRepo;
        private Mock<ISupportsModelPersistence> mockService1;
        private Mock<ISupportsModelPersistence> mockService2;
        private IEnumerable<ISupportsModelPersistence> mockServices;
        private ApplicationDatabaseService subject;

        [TestMethod]
        [ExpectedException(typeof(EncryptionKeyNotProvidedException))]
        public async Task EncryptFilesAsync_ShouldThrow_GivenNoClaimSet()
        {
            await this.subject.EncryptFilesAsync();

            Assert.Fail();
        }

        [TestMethod]
        public void SetClaim_ShouldCallCredentialStore_GivenValidClaim()
        {
            this.subject.SetCredential(CredentialStoreTest.CreateSecureString("Foo"));

            this.mockCredentials.Verify(m => m.SetPasskey(It.IsAny<SecureString>()));
        }

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

            await this.subject.CreateNewDatabaseAsync("Foo");
            this.subject.NotifyOfChange(ApplicationDataType.Budget);
            Assert.IsTrue(this.subject.HasUnsavedChanges);
            this.subject.Close();

            Assert.IsFalse(this.subject.HasUnsavedChanges);
        }

        [TestMethod]
        public void Close_ShouldReturnNull_GivenNoApplicationDatabaseIsLoaded()
        {
            Assert.IsNull(this.subject.Close());
        }

        [TestMethod]
        public async Task CreateNewDatabaseAsync_ShouldCallCreateOnEachService()
        {
            CreateNewDatabaseSetup();

            var appDb = await this.subject.CreateNewDatabaseAsync("Foo");

            this.mockService1.Verify(m => m.CreateAsync(It.IsAny<ApplicationDatabase>()));
            this.mockService2.Verify(m => m.CreateAsync(It.IsAny<ApplicationDatabase>()));
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
            var services = (IEnumerable<ISupportsModelPersistence>) PrivateAccessor.GetField(this.subject, "databaseDependents");
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
            new ApplicationDatabaseService(null, this.mockServices, new FakeMonitorableDependencies(), this.mockCredentials.Object, new FakeLogger());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_ShouldThrow_GivenNullServices()
        {
            new ApplicationDatabaseService(this.mockRepo.Object, null, new FakeMonitorableDependencies(), this.mockCredentials.Object, new FakeLogger());
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
        public void NotifyOfChange_ShouldIndicateUnsavedChanges()
        {
            foreach (var dataType in Enum.GetValues(typeof(ApplicationDataType)))
            {
                this.subject.NotifyOfChange((ApplicationDataType) dataType);
                Assert.IsTrue(this.subject.HasUnsavedChanges);
                TestInitialise();
            }
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
        public async Task Save_ShouldResetUnSavedChanges()
        {
            SaveSetup();
            await this.subject.SaveAsync();

            Assert.IsFalse(this.subject.HasUnsavedChanges);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Save_ShouldThrow_GivenNothingLoaded()
        {
            await this.subject.SaveAsync();
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
            this.subject = new ApplicationDatabaseService(this.mockRepo.Object, this.mockServices, new FakeMonitorableDependencies(), this.mockCredentials.Object, new FakeLogger());
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

            this.subject.NotifyOfChange(ApplicationDataType.Budget);
        }
    }
}