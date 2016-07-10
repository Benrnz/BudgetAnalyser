﻿using System;
using System.Security;
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
        private const string Password = "Password123456789";
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
            foreach (var c in Password.ToCharArray())
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
        public async Task LoadTest()
        {
            var budgets = await this.subject.LoadAsync(StorageKey);
            Console.WriteLine(this.securePassPhrase);
            Assert.AreNotEqual(0, budgets.Count);
        }

        [TestMethod]
        public async Task SaveTest()
        {
            var budgets = await this.subject.LoadAsync(StorageKey);
            await this.subject.SaveAsync();
        }
    }
}
