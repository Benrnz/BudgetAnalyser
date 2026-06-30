#nullable disable
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.XUnit.TestData;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

public class LedgerBucketTest
{
    [Fact]
    public void DictionaryTest()
    {
        var instance1 = LedgerBookTestData.HairLedger;

        var dictionary = new Dictionary<LedgerBucket, LedgerBucket>
        {
            { instance1, instance1 }
        };

        LedgerBucket instance2 = new SavedUpForLedger
        {
            BudgetBucket = TransactionsListModelTestData.HairBucket,
            StoredInAccount = LedgerBookTestData.ChequeAccount
        };

        // Should already contain this, its the same code.
        dictionary.ContainsKey(instance2).ShouldBeTrue();

        LedgerBucket instance3 = new SavedUpForLedger
        {
            BudgetBucket = new SavedUpForExpenseBucket("HAIRCUT", "Foo bar"),
            StoredInAccount = LedgerBookTestData.ChequeAccount
        };

        dictionary.ContainsKey(instance3).ShouldBeTrue();
    }

    [Fact]
    public void TwoInstancesWithDifferentBucketAndSameAccountsAreNotEqual()
    {
        var bucket1 = new SpentPerPeriodExpenseBucket("Foo1", "Foo bar1");
        var bucket2 = new SpentPerPeriodExpenseBucket("Foo2", "Foo bar2");

        var instance1 = new SpentPerPeriodLedger
        {
            BudgetBucket = bucket1,
            StoredInAccount = TransactionsListModelTestData.ChequeAccount
        };
        var instance2 = new SpentPerPeriodLedger
        {
            BudgetBucket = bucket2,
            StoredInAccount = TransactionsListModelTestData.ChequeAccount
        };
        instance2.ShouldNotBe(instance1);
        (instance1 == instance2).ShouldBeFalse();
    }

    [Fact]
    public void TwoInstancesWithEquivalentBucketAndAccountAreEqual()
    {
        var bucket1 = new SpentPerPeriodExpenseBucket("Foo1", "Foo bar");
        var bucket2 = new SpentPerPeriodExpenseBucket("Foo1", "Foo bar");

        var instance1 = new SpentPerPeriodLedger
        {
            BudgetBucket = bucket1,
            StoredInAccount = TransactionsListModelTestData.SavingsAccount
        };
        var instance2 = new SpentPerPeriodLedger
        {
            BudgetBucket = bucket2,
            StoredInAccount = TransactionsListModelTestData.SavingsAccount
        };
        instance2.ShouldBe(instance1);
        (instance1 == instance2).ShouldBeTrue();
    }

    [Fact]
    public void TwoInstancesWithEquivalentBucketAndDifferentAccountsAreNotEqual()
    {
        var bucket1 = new SpentPerPeriodExpenseBucket("Foo1", "Foo bar");
        var bucket2 = new SpentPerPeriodExpenseBucket("Foo1", "Foo bar");

        var instance1 = new SpentPerPeriodLedger
        {
            BudgetBucket = bucket1,
            StoredInAccount = TransactionsListModelTestData.ChequeAccount
        };
        var instance2 = new SpentPerPeriodLedger
        {
            BudgetBucket = bucket2,
            StoredInAccount = TransactionsListModelTestData.SavingsAccount
        };
        instance2.ShouldNotBe(instance1);
        (instance1 == instance2).ShouldBeFalse();
    }

    [Fact]
    public void TwoInstancesWithSameBucketAndAccountAreEqual()
    {
        var instance1 = new SavedUpForLedger
        {
            BudgetBucket = TransactionsListModelTestData.CarMtcBucket,
            StoredInAccount = TransactionsListModelTestData.SavingsAccount
        };
        var instance2 = new SavedUpForLedger
        {
            BudgetBucket = TransactionsListModelTestData.CarMtcBucket,
            StoredInAccount = TransactionsListModelTestData.SavingsAccount
        };
        instance2.ShouldBe(instance1);
        (instance1 == instance2).ShouldBeTrue();
    }

    [Fact]
    public void TwoInstancesWithSameBucketAndEquivalentAccountAreEqual()
    {
        var account1 = new SavingsAccount("Foo");
        var account2 = new SavingsAccount("Foo");

        var instance1 = new SavedUpForLedger
        {
            BudgetBucket = TransactionsListModelTestData.CarMtcBucket,
            StoredInAccount = account1
        };
        var instance2 = new SavedUpForLedger
        {
            BudgetBucket = TransactionsListModelTestData.CarMtcBucket,
            StoredInAccount = account2
        };
        instance2.ShouldBe(instance1);
        (instance1 == instance2).ShouldBeTrue();
    }
}
