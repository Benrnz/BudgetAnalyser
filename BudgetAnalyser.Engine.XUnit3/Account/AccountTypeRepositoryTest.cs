using BudgetAnalyser.Engine.BankAccount;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Account;

public class AccountTypeRepositoryTest
{
    private const string Key1 = AccountTypeRepositoryConstants.Cheque;
    private const string Key2 = AccountTypeRepositoryConstants.Visa;

    [Fact]
    public void AddingDuplicateEntryShouldNotThrow()
    {
        var subject = CreateSubject();
        var data = CreateTestData2();
        subject.Add(Key1, data);
        var data2 = CreateTestData2();
        subject.Add(Key1, data2);
        var result = subject.GetByKey(Key1);
        result.ShouldNotBeNull();
        result.Name.ShouldBe(data.Name);
    }

    [Fact]
    public void AddingEmptyKeyEntryShouldThrow()
    {
        var subject = CreateSubject();

        Should.Throw<ArgumentNullException>(() => subject.Add(string.Empty, CreateTestData()));
    }

    [Fact]
    public void AddingNewEntryShouldBeRetrievableByKey()
    {
        var subject = CreateSubject();
        var data = CreateTestData2();
        subject.Add(Key1, data);
        var result = subject.GetByKey(Key1);
        result.ShouldNotBeNull();
        result.Name.ShouldBe(data.Name);
    }

    [Fact]
    public void AddingNullEntryShouldThrow()
    {
        var subject = CreateSubject();

        Should.Throw<ArgumentNullException>(() => subject.Add(Key1, null!));
    }

    [Fact]
    public void AddingNullKeyEntryShouldThrow()
    {
        var subject = CreateSubject();

        Should.Throw<ArgumentNullException>(() => subject.Add(null!, CreateTestData()));
    }

    [Fact]
    public void FindExistingValueShouldSucceed()
    {
        var subject = CreateSubject();
        subject.Add(Key1, CreateTestData());
        subject.Add(Key2, CreateTestData());

        var result = subject.Find(a => a.Name == Key1);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void FindNonExistentValueShouldFail()
    {
        var subject = CreateSubject();
        subject.Add(Key1, CreateTestData());
        subject.Add(Key2, CreateTestData());

        var result = subject.Find(a => a.Name == "Key99");

        result.ShouldBeNull();
    }

    [Fact]
    public void GetOrCreateNewDuplicateEntryShouldNotThrow()
    {
        var subject = CreateSubject();
        var result1 = subject.GetByKey(Key1);
        var result2 = subject.GetByKey(Key1);

        ReferenceEquals(result1, result2).ShouldBeTrue();
    }

    private static InMemoryAccountTypeRepository CreateSubject()
    {
        return new InMemoryAccountTypeRepository();
    }

    private static AmexAccount CreateTestData()
    {
        return new AmexAccount(AccountTypeRepositoryConstants.Amex);
    }

    private static ChequeAccount CreateTestData2()
    {
        return new ChequeAccount(AccountTypeRepositoryConstants.Cheque);
    }
}
