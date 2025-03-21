﻿using System.Text;

namespace BudgetAnalyser.Engine.UnitTest;

[TestClass]
public class GlobalFilterCriteriaTest
{
    private readonly StringBuilder validationMessages = new();

    [TestMethod]
    public void CheckReferenceEquality()
    {
        var subject1 = CreateSubject_StandardPayMonth();
        var subject2 = subject1;

        Assert.AreEqual(subject1, subject2);
        Assert.IsFalse(subject1 != subject2);
        Assert.IsTrue(subject1 == subject2);
        Assert.IsTrue(subject1.Equals(subject2));
        Assert.AreSame(subject1, subject2);
        Assert.AreEqual(subject1.GetHashCode(), subject2.GetHashCode());
    }

    [TestMethod]
    public void CheckReferenceInequality()
    {
        var subject1 = CreateSubject_StandardPayMonth();
        var subject2 = CreateSubject_NotFiltered();

        Assert.AreNotEqual(subject1, subject2);
        Assert.IsTrue(subject1 != subject2);
        Assert.IsFalse(subject1 == subject2);
        Assert.IsFalse(subject1.Equals(subject2));
        Assert.AreNotSame(subject1, subject2);
        Assert.AreNotEqual(subject1.GetHashCode(), subject2.GetHashCode());
    }

    [TestMethod]
    public void CtorShouldSetClearedToTrue()
    {
        Assert.IsTrue(new GlobalFilterCriteria().Cleared);
    }

    [TestMethod]
    public void EndDateShouldAutoAdjustGivenDateAfterStartDate()
    {
        var invalidEndDate = new DateOnly(2014, 3, 1);
        var subject = new GlobalFilterCriteria { BeginDate = new DateOnly(2014, 5, 1), EndDate = invalidEndDate };

        Assert.AreNotEqual(invalidEndDate, subject.EndDate);
        Assert.IsTrue(subject.BeginDate < subject.EndDate);
    }

    [TestMethod]
    public void MinDateShouldGetTransalatedToNull()
    {
        var subject = new GlobalFilterCriteria { BeginDate = DateOnly.MinValue, EndDate = DateOnly.MinValue };

        Assert.IsNull(subject.BeginDate);
        Assert.IsNull(subject.EndDate);
    }

    [TestMethod]
    public void TwoInstancesWithDifferentValuesAreNotEqual()
    {
        var subject1 = CreateSubject_StandardPayMonth();
        var subject2 = CreateSubject_StandardPayMonthAndVisaFilter();

        Assert.AreNotEqual(subject1, subject2);
        Assert.IsTrue(subject1 != subject2);
        Assert.IsFalse(subject1 == subject2);
        Assert.IsFalse(subject1.Equals(subject2));
        Assert.AreNotSame(subject1, subject2);
        Assert.AreNotEqual(subject1.GetHashCode(), subject2.GetHashCode());
    }

    [TestMethod]
    public void TwoInstancesWithSameValuesShouldHaveSameEqualityHash()
    {
        var subject1 = CreateSubject_StandardPayMonth();
        var subject2 = CreateSubject_StandardPayMonth();

        Assert.AreEqual(subject1.SignificantDataChangeHash(), subject2.SignificantDataChangeHash());
        Assert.IsFalse(subject1.SignificantDataChangeHash() != subject2.SignificantDataChangeHash());
        Assert.IsTrue(subject1.SignificantDataChangeHash() == subject2.SignificantDataChangeHash());
        Assert.IsTrue(subject1.SignificantDataChangeHash().Equals(subject2.SignificantDataChangeHash()));
        Assert.AreNotSame(subject1, subject2);
        Assert.AreNotEqual(subject1.GetHashCode(), subject2.GetHashCode());
    }

    [TestMethod]
    public void ValidateShouldReturnFalseGivenBeginDateIsNull()
    {
        var subject = new GlobalFilterCriteria { BeginDate = new DateOnly(), EndDate = DateOnlyExt.Today() };
        subject.BeginDate = null;
        Assert.IsFalse(subject.Validate(this.validationMessages));
    }

    [TestMethod]
    public void ValidateShouldReturnFalseGivenEndDateIsNull()
    {
        var subject = new GlobalFilterCriteria { BeginDate = DateOnlyExt.Today().AddDays(-1), EndDate = DateOnlyExt.Today() };
        subject.EndDate = null;
        Assert.IsFalse(subject.Validate(this.validationMessages));
    }

    [TestMethod]
    public void ValidateShouldReturnTrueGivenCleared()
    {
        var subject = new GlobalFilterCriteria();
        Assert.IsTrue(subject.Cleared);
        Assert.IsTrue(subject.Validate(this.validationMessages));
    }

    [TestMethod]
    public void ValidateShouldReturnTrueGivenDatesButNoAccount()
    {
        var subject = new GlobalFilterCriteria { BeginDate = DateOnlyExt.Today().AddDays(-1), EndDate = DateOnlyExt.Today() };
        Assert.IsFalse(subject.Cleared);
        Assert.IsTrue(subject.Validate(this.validationMessages));
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ValidateShouldThrowGivenNullValidationMessages()
    {
        var subject = new GlobalFilterCriteria { BeginDate = new DateOnly(), EndDate = DateOnlyExt.Today() };
        Assert.IsFalse(subject.Validate(null));
    }

    [TestMethod]
    public void WhenNotFilteredClearedShouldBeTrue()
    {
        var subject1 = CreateSubject_NotFiltered();
        Assert.IsTrue(subject1.Cleared);
    }

    private GlobalFilterCriteria CreateSubject_NotFiltered()
    {
        return new GlobalFilterCriteria { BeginDate = null, EndDate = null };
    }

    private GlobalFilterCriteria CreateSubject_StandardPayMonth()
    {
        return new GlobalFilterCriteria { BeginDate = new DateOnly(2014, 1, 20), EndDate = new DateOnly(2014, 2, 19) };
    }

    private GlobalFilterCriteria CreateSubject_StandardPayMonthAndVisaFilter()
    {
        return new GlobalFilterCriteria { BeginDate = new DateOnly(2014, 1, 20), EndDate = new DateOnly(2014, 2, 19) };
    }
}
