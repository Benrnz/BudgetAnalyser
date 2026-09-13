using System.Diagnostics;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Transactions;

public class TransactionsListModelTest
{
    private readonly BankAccount.Account cheque = new ChequeAccount("Cheque");
    private readonly BudgetBucket expense1 = new SavedUpForExpenseBucket(TestDataConstants.CarMtcBucketCode, string.Empty);
    private readonly BudgetBucket income = new IncomeBudgetBucket(TestDataConstants.IncomeBucketCode, string.Empty);
    private readonly BudgetBucket surplus = new SurplusBucket();
    private readonly TransactionType transactionType = new NamedTransaction("Something");
    private readonly BankAccount.Account visa = new VisaAccount("Visa");

    private Transaction Duplicate1 => new()
    {
        Account = this.cheque, Amount = -10.01M, BudgetBucket = this.surplus, Date = new DateOnly(2013, 1, 1), Description = "abcdefghi", Reference1 = "1", Reference2 = "2", Reference3 = "3",
        TransactionType = this.transactionType
    };

    private Transaction Duplicate2 => new()
    {
        Account = this.visa, Amount = -99.11M, BudgetBucket = this.expense1, Date = new DateOnly(2013, 1, 3), Description = "Foo bar", Reference1 = "1", Reference2 = "2", Reference3 = "3",
        TransactionType = this.transactionType
    };

    private Transaction Duplicate3 => new()
    {
        Account = this.cheque, Amount = 1000.01M, BudgetBucket = this.income, Date = new DateOnly(2013, 1, 1), Description = "Salary", Reference1 = "1", Reference2 = "2", Reference3 = "3",
        TransactionType = this.transactionType
    };

    private Transaction Transaction1 => new()
    {
        Account = this.cheque, Amount = -10.01M, BudgetBucket = this.surplus, Date = new DateOnly(2013, 1, 1), Description = "abcdefghi", Reference1 = "1", Reference2 = "2", Reference3 = "3",
        TransactionType = this.transactionType
    };

    private Transaction Transaction2 => new()
    {
        Account = this.visa, Amount = -99.11M, BudgetBucket = this.expense1, Date = new DateOnly(2013, 1, 3), Description = "Foo bar", Reference1 = "1", Reference2 = "2", Reference3 = "3",
        TransactionType = this.transactionType
    };

    private Transaction Transaction3 => new()
    {
        Account = this.cheque, Amount = 1000.01M, BudgetBucket = this.income, Date = new DateOnly(2013, 1, 1), Description = "Salary", Reference1 = "1", Reference2 = "2", Reference3 = "3",
        TransactionType = this.transactionType
    };

    [Fact]
    public void PerformanceOfValidateTest()
    {
        var subject = TransactionsListModelTestData.TestData1();
        var stopwatch = Stopwatch.StartNew();
        subject.ValidateAgainstDuplicates();
        stopwatch.Stop();

        Console.WriteLine("{0:N0} ms", stopwatch.ElapsedMilliseconds);
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(500);
    }

    [Fact]
    public void ValidateShouldFailWhenDuplicates1()
    {
        new TransactionsListModel(new FakeLogger()).LoadTransactions([Transaction1, Transaction2, Transaction3, Duplicate1]).ValidateAgainstDuplicates().Any().ShouldBeTrue();
    }

    [Fact]
    public void ValidateShouldFailWhenDuplicates2()
    {
        new TransactionsListModel(new FakeLogger()).LoadTransactions([Transaction1, Transaction2, Duplicate2, Transaction3]).ValidateAgainstDuplicates().Any().ShouldBeTrue();
    }

    [Fact]
    public void ValidateShouldFailWhenDuplicates3()
    {
        new TransactionsListModel(new FakeLogger()).LoadTransactions([Duplicate3, Transaction1, Transaction2, Transaction3]).ValidateAgainstDuplicates().Any().ShouldBeTrue();
    }

    [Fact]
    public void ValidateShouldPassWhenNoDuplicates()
    {
        new TransactionsListModel(new FakeLogger()).LoadTransactions([Transaction1, Transaction2, Transaction3]).ValidateAgainstDuplicates().Any().ShouldBeFalse();
    }
}
