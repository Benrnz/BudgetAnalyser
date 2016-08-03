//using System;
//using System.Security;
//using System.Threading.Tasks;
//using BudgetAnalyser.Engine.Budget;
//using BudgetAnalyser.Engine.Budget.Data;
//using BudgetAnalyser.Storage;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace BudgetAnalyser.Wpf.UnitTest.Storage
//{
//    [TestClass]
//    [Ignore]
//    public class EncryptedXamlOnDiskBudgetRepoTest
//    {
//        // TODO delete this when done.
//        private const string StorageKey = @"D:\";

//        private IBudgetRepository subject;
//        private IBudgetBucketRepository bucketRepo;
//        private readonly SecureString securePassPhrase = new SecureString();
//        private BudgetBucketFactory budgetBucketFactory;
//        private Mapper_BudgetBucketDto_BudgetBucket bucketMapper;
//        private Mapper_BudgetModelDto_BudgetModel budgetMapper;
//        private Mapper_BudgetCollectionDto_BudgetCollection collectionMapper;

//        [TestInitialize]
//        public void TestSetup()
//        {
//            this.budgetBucketFactory = new BudgetBucketFactory();
//            this.bucketMapper = new Mapper_BudgetBucketDto_BudgetBucket(this.budgetBucketFactory);
//            this.bucketRepo = new InMemoryBudgetBucketRepository(this.bucketMapper);
//            this.budgetMapper = new Mapper_BudgetModelDto_BudgetModel(this.bucketRepo);
//            this.collectionMapper = new Mapper_BudgetCollectionDto_BudgetCollection(this.bucketRepo, this.bucketMapper, this.budgetMapper);
//            //this.subject = new EncryptedXamlOnDiskBudgetRepository(this.bucketRepo, this.collectionMapper, new FileEncryptor(), new CredentialStore());
//        }

//        [TestMethod]
//        public async Task LoadTest()
//        {
//            var budgets = await this.subject.LoadAsync(StorageKey, true);
//            Console.WriteLine(this.securePassPhrase);
//            Assert.AreNotEqual(0, budgets.Count);
//        }

//        [TestMethod]
//        public async Task SaveTest()
//        {
//            var budgets = await this.subject.LoadAsync(StorageKey, true);
//            await this.subject.SaveAsync();
//        }
//    }
//}
