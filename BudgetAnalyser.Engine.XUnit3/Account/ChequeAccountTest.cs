using BudgetAnalyser.Engine.BankAccount;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Account;

public class ChequeAccountTest
{
    [Fact]
    public void CloneShouldGiveUseNameGiven()
    {
        var subject = CreateSubject();
        var clone = subject.Clone("CloneCheque");

        clone.Name.ShouldBe("CloneCheque");
        clone.Name.ShouldNotBe(subject.Name);
    }

    [Fact]
    public void CloneShouldNotJustCopyReference()
    {
        var subject = CreateSubject();
        var clone = subject.Clone("CloneCheque");

        ReferenceEquals(subject, clone).ShouldBeFalse();
    }

    [Fact]
    public void KeywordsShouldContainElements()
    {
        var subject = CreateSubject();

        subject.KeyWords.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void KeywordsShouldNotBeNull()
    {
        var subject = CreateSubject();

        subject.KeyWords.ShouldNotBeNull();
    }

    [Fact]
    public void NameShouldBeSomething()
    {
        var subject = CreateSubject();

        string.IsNullOrWhiteSpace(subject.Name).ShouldBeFalse();
    }

    private static ChequeAccount CreateSubject()
    {
        return new ChequeAccount("ChequeTest");
    }
}
