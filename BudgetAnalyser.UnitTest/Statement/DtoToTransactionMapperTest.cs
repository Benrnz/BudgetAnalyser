using System;
using AutoMapper;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Statement.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Statement
{
    [TestClass]
    public class DtoToTransactionMapperTest
    {
        private static readonly Guid TransactionId = new Guid("7F921750-4467-4EA4-81E6-3EFD466341C6");
        private Transaction Result { get; set; }

        private TransactionDto TestData
        {
            get
            {
                return new TransactionDto
                {
                    Id = TransactionId,
                    Account = StatementModelTestData.ChequeAccount.Name,
                    Amount = 123.99M,
                    BudgetBucketCode = TestDataConstants.PowerBucketCode,
                    Date = new DateTime(2014, 07, 31),
                    Description = "The quick brown poo",
                    Reference1 = "Reference 1",
                    Reference2 = "REference 23",
                    Reference3 = "REference 33",
                    TransactionType = "Credit Card Debit"
                };
            }
        }

        [TestMethod]
        public void ShouldMapAccountType()
        {
            Assert.AreEqual(TestData.Account, Result.Account.Name);
        }

        [TestMethod]
        public void ShouldMapAmount()
        {
            Assert.AreEqual(TestData.Amount, Result.Amount);
        }

        [TestMethod]
        public void ShouldMapBucketCode()
        {
            Assert.AreEqual(TestData.BudgetBucketCode, Result.BudgetBucket.Code);
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
            Assert.AreEqual(TestData.TransactionType, Result.TransactionType.Name);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            Result = Mapper.Map<Transaction>(TestData);
        }
    }
}