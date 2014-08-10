using System;
using AutoMapper;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Statement.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Statement
{
    [TestClass]
    public class TransactionToDtoMapperTest
    {
        private static readonly Guid TransactionId = new Guid("7F921750-4467-4EA4-81E6-3EFD466341C6");

        private TransactionDto Result { get; set; }

        private Transaction TestData
        {
            get
            {
                return new Transaction
                {
                    Id = TransactionId,
                    AccountType = StatementModelTestData.ChequeAccount,
                    Amount = 123.99M,
                    BudgetBucket = StatementModelTestData.PowerBucket,
                    Date = new DateTime(2014, 07, 31),
                    Description = "The quick brown poo",
                    Reference1 = "Reference 1",
                    Reference2 = "REference 23",
                    Reference3 = "REference 33",
                    TransactionType = StatementModelTestData.TransactionType,
                };
            }
        }

        [TestMethod]
        public void ShouldMapAccountType()
        {
            Assert.AreEqual(TestData.AccountType.Name, Result.AccountType);
        }

        [TestMethod]
        public void ShouldMapAmount()
        {
            Assert.AreEqual(TestData.Amount, Result.Amount);
        }

        [TestMethod]
        public void ShouldMapBucketCode()
        {
            Assert.AreEqual(TestData.BudgetBucket.Code, Result.BudgetBucketCode);
        }

        [TestMethod]
        public void ShouldMapDate()
        {
            Assert.AreEqual(TestData.Date, Result.Date);
        }

        [TestMethod]
        public void ShouldMapDescription()
        {
            Assert.AreEqual(TestData.Description, Result.Description);
        }

        [TestMethod]
        public void ShouldMapId()
        {
            Assert.AreEqual(TransactionId, Result.Id);
        }

        [TestMethod]
        public void ShouldMapReference1()
        {
            Assert.AreEqual(TestData.Reference1, Result.Reference1);
        }

        [TestMethod]
        public void ShouldMapReference2()
        {
            Assert.AreEqual(TestData.Reference2, Result.Reference2);
        }

        [TestMethod]
        public void ShouldMapReference3()
        {
            Assert.AreEqual(TestData.Reference3, Result.Reference3);
        }

        [TestMethod]
        public void ShouldMapTransactionType()
        {
            Assert.AreEqual(TestData.TransactionType.Name, Result.TransactionType);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            

            Result = Mapper.Map<TransactionDto>(TestData);
        }
    }
}