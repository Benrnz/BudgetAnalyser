using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Wpf.UnitTest.Storage
{
    [TestClass]
    public class EncryptedXamlOnDiskBudgetRepoTest
    {
        private const string StorageKey = @"D:\Development\ReesAccounts\ReesBudget2013.3.xml";
        private IBudgetRepository subject;
        private IBudgetBucketRepository bucketRepo;
        private readonly SecureString securePassPhrase = new SecureString();
        private BudgetBucketFactory budgetBucketFactory;
        private Mapper_BudgetBucketDto_BudgetBucket bucketMapper;
        private Mapper_BudgetModelDto_BudgetModel budgetMapper;
        private Mapper_BudgetCollectionDto_BudgetCollection collectionMapper;

        [TestInitialize]
        public void TestSetup()
        {
            string password = "Password123";
            foreach (var c in password.ToCharArray())
            {
                this.securePassPhrase.AppendChar(c);
            }

            this.budgetBucketFactory = new BudgetBucketFactory();
            this.bucketMapper = new Mapper_BudgetBucketDto_BudgetBucket(this.budgetBucketFactory);
            this.bucketRepo = new InMemoryBudgetBucketRepository(this.bucketMapper);
            this.budgetMapper = new Mapper_BudgetModelDto_BudgetModel(this.bucketRepo);
            this.collectionMapper = new Mapper_BudgetCollectionDto_BudgetCollection(this.bucketRepo, this.bucketMapper, this.budgetMapper);
            this.subject = new EncryptedXamlOnDiskBudgetRepository(this.bucketRepo, this.collectionMapper, this.securePassPhrase);

        }

        [TestMethod]
        public async Task ConvertExisting()
        {
            var xamlBudgetRepo = new XamlOnDiskBudgetRepository(this.bucketRepo, this.collectionMapper);
            var budgets = await xamlBudgetRepo.LoadAsync(StorageKey);

            await this.subject.SaveAsync();
        }

        [TestMethod]
        public async Task LoadTest()
        {
            try
            {
                var budgets = await this.subject.LoadAsync(StorageKey);
                Assert.AreNotEqual(0, budgets.Count);
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [TestMethod]
        public void SaveTest()
        {
            
        }
    }
}
