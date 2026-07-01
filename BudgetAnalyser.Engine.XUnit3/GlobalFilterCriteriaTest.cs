using System.Text;
using Shouldly;
namespace BudgetAnalyser.Engine.XUnit;

public class GlobalFilterCriteriaTest
{
    private readonly StringBuilder validationMessages = new();

    [Fact]
    public void CheckReferenceEquality()
    {
        var subject1 = CreateSubject_StandardPayMonth();
        var subject2 = subject1;

        subject1.ShouldBeSameAs(subject2);
        (subject1 != subject2).ShouldBeFalse();
        (subject1 == subject2).ShouldBeTrue();
        subject1.Equals(subject2).ShouldBeTrue();
        subject1.ShouldBeSameAs(subject2);
        subject1.GetHashCode().ShouldBe(subject2.GetHashCode());
    }

    [Fact]
    public void CheckReferenceInequality()
    {
        var subject1 = CreateSubject_StandardPayMonth();
        var subject2 = CreateSubject_NotFiltered();

        subject1.ShouldNotBe(subject2);
        (subject1 != subject2).ShouldBeTrue();
        (subject1 == subject2).ShouldBeFalse();
        subject1.Equals(subject2).ShouldBeFalse();
        subject1.ShouldNotBeSameAs(subject2);
        subject1.GetHashCode().ShouldNotBe(subject2.GetHashCode());
    }

    [Fact]
    public void CtorShouldSetClearedToTrue()
    {
        new GlobalFilterCriteria().Cleared.ShouldBeTrue();
    }

    [Fact]
    public void EndDateShouldAutoAdjustGivenDateAfterStartDate()
    {
        var invalidEndDate = new DateOnly(2014, 3, 1);
        var subject = new GlobalFilterCriteria { BeginDate = new DateOnly(2014, 5, 1), EndDate = invalidEndDate };

        subject.EndDate.ShouldNotBe(invalidEndDate);
        (subject.BeginDate < subject.EndDate).ShouldBeTrue();
    }

    [Fact]
    public void MinDateShouldGetTransalatedToNull()
    {
        var subject = new GlobalFilterCriteria { BeginDate = DateOnly.MinValue, EndDate = DateOnly.MinValue };

        subject.BeginDate.ShouldBeNull();
        subject.EndDate.ShouldBeNull();
    }

    [Fact]
    public void TwoInstancesWithSameValuesShouldHaveSameEqualityHash()
    {
        var subject1 = CreateSubject_StandardPayMonth();
        var subject2 = CreateSubject_StandardPayMonth();

        subject1.SignificantDataChangeHash().ShouldBe(subject2.SignificantDataChangeHash());
        (subject1.SignificantDataChangeHash() != subject2.SignificantDataChangeHash()).ShouldBeFalse();
        (subject1.SignificantDataChangeHash() == subject2.SignificantDataChangeHash()).ShouldBeTrue();
        subject1.SignificantDataChangeHash().Equals(subject2.SignificantDataChangeHash()).ShouldBeTrue();
        subject1.ShouldNotBeSameAs(subject2);
        subject1.GetHashCode().ShouldNotBe(subject2.GetHashCode());
    }

    [Fact]
    public void ValidateShouldReturnFalseGivenBeginDateIsNull()
    {
        var subject = new GlobalFilterCriteria { BeginDate = new DateOnly(), EndDate = DateOnlyExt.Today() };
        subject.BeginDate = null;
        subject.Validate(this.validationMessages).ShouldBeFalse();
    }

    [Fact]
    public void ValidateShouldReturnFalseGivenEndDateIsNull()
    {
        var subject = new GlobalFilterCriteria { BeginDate = DateOnlyExt.Today().AddDays(-1), EndDate = DateOnlyExt.Today() };
        subject.EndDate = null;
        subject.Validate(this.validationMessages).ShouldBeFalse();
    }

    [Fact]
    public void ValidateShouldReturnTrueGivenCleared()
    {
        var subject = new GlobalFilterCriteria();
        subject.Cleared.ShouldBeTrue();
        subject.Validate(this.validationMessages).ShouldBeTrue();
    }

    [Fact]
    public void ValidateShouldReturnTrueGivenDatesButNoAccount()
    {
        var subject = new GlobalFilterCriteria { BeginDate = DateOnlyExt.Today().AddDays(-1), EndDate = DateOnlyExt.Today() };
        subject.Cleared.ShouldBeFalse();
        subject.Validate(this.validationMessages).ShouldBeTrue();
    }

    [Fact]
    public void ValidateShouldThrowGivenNullValidationMessages()
    {
        var subject = new GlobalFilterCriteria { BeginDate = new DateOnly(), EndDate = DateOnlyExt.Today() };
        Should.Throw<ArgumentNullException>(() => subject.Validate(null!));
    }

    [Fact]
    public void WhenNotFilteredClearedShouldBeTrue()
    {
        var subject1 = CreateSubject_NotFiltered();
        subject1.Cleared.ShouldBeTrue();
    }

    private GlobalFilterCriteria CreateSubject_NotFiltered()
    {
        return new GlobalFilterCriteria { BeginDate = null, EndDate = null };
    }

    private GlobalFilterCriteria CreateSubject_StandardPayMonth()
    {
        return new GlobalFilterCriteria { BeginDate = new DateOnly(2014, 1, 20), EndDate = new DateOnly(2014, 2, 19) };
    }
}
