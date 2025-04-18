﻿using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Statement.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;

namespace BudgetAnalyser.Engine.UnitTest.Statement;

[TestClass]
public class DtoToStatementModelMapperTest
{
    private StatementModel Result { get; set; }
    private TransactionSetDto TestData => TransactionSetDtoTestData.TestData2();

    [TestMethod]
    public void ChangeHashShouldNotBeNull()
    {
        Assert.IsNotNull(Result.SignificantDataChangeHash());
    }

    [TestMethod]
    public void ShouldBeUnfiltered()
    {
        Assert.IsFalse(Result.Filtered);
    }

    [TestMethod]
    public void ShouldMapAllTransactions()
    {
        Assert.AreEqual(TestData.Transactions.Count(), Result.AllTransactions.Count());
    }

    [TestMethod]
    public void ShouldMapAllTransactionsAndHaveSameSum()
    {
        Assert.AreEqual(TestData.Transactions.Sum(t => t.Amount), Result.AllTransactions.Sum(t => t.Amount));
        Assert.AreEqual(TestData.Transactions.Sum(t => t.Date.DayNumber), Result.AllTransactions.Sum(t => t.Date.DayNumber));
    }

    [TestMethod]
    public void ShouldMapDurationInMonths()
    {
        Assert.AreEqual(2, Result.DurationInMonths);
    }

    //        [TestMethod]
    //        public void CodifyTestData2()
    //        {
    //            var testData = StatementModelTestData.TestData2();
    //            var mapper = new StatementModelToTransactionSetDtoMapper(new TransactionToTransactionDtoMapper());
    //            var result = mapper.Map(testData, "skjgiuwguih2798yrg972hguoisi7fgiusgfs", @"C:\Foo\Bar.csv");

    //            Console.WriteLine(@"
    //public TransactionSetDto TestData2() {{
    //    return new TransactionSetDto() {{
    //        Checksum = 252523523525,
    //        StorageKey = ""{0}"",
    //        LastImport = new DateTime({1}, {2}, {3}),
    //        VersionHash = ""uiwhgr8972y59872gh5972798gh"",
    //        Transactions = new List<TransactionDto>
    //        {{ {4} }},
    //    }};
    //}}
    //", result.StorageKey, result.LastImport.Year, result.LastImport.Month, result.LastImport.Day, GenerateTransactionsCode(result));

    //        }

    //        private string GenerateTransactionsCode(TransactionSetDto result)
    //        {
    //            var builder = new StringBuilder();
    //            foreach (var txn in result.Transactions)
    //            {
    //                builder.AppendFormat(@"
    //            new TransactionDto
    //            {{
    //                Account = ""{0}"",
    //                Amount = {1}M,
    //                BudgetBucketCode = ""{2}"",
    //                Date = new DateTime({3}, {4}, {5}),
    //                Description = ""{6}"",
    //                Id = new Guid(""{7}""),
    //                Reference1 = ""{8}"",
    //                Reference2 = ""{9}"",
    //                Reference3 = ""{10}"",
    //                TransactionType = ""{11}"",
    //            }},
    //", txn.Account, txn.Amount, txn.BudgetBucketCode, txn.Date.Year, txn.Date.Month, txn.Date.Day, txn.Description, txn.Id, txn.Reference1, txn.Reference2, txn.Reference3, txn.TransactionType);
    //            }

    //            return builder.ToString();
    //        }

    [TestMethod]
    public void ShouldMapFileName()
    {
        Assert.AreEqual(TestData.StorageKey, Result.StorageKey);
    }

    [TestMethod]
    public void ShouldMapLastImport()
    {
        Assert.AreEqual(TestData.LastImport, Result.LastImport.ToUniversalTime());
    }

    [TestInitialize]
    public void TestInitialise()
    {
        var subject = new MapperStatementModelToDto2(
            new InMemoryAccountTypeRepository(),
            new BucketBucketRepoAlwaysFind(),
            new InMemoryTransactionTypeRepository(),
            new FakeLogger());
        Result = subject.ToModel(TestData);
    }

    [TestMethod]
    public void TransactionsShouldBeInAscendingOrder()
    {
        var previous = DateOnly.MinValue;
        foreach (var txn in Result.AllTransactions)
        {
            if (txn.Date < previous)
            {
                Assert.Fail();
            }

            previous = txn.Date;
        }
    }
}
