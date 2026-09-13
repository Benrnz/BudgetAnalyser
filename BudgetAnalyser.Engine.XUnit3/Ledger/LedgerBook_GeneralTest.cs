#nullable disable
using System.Text;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.XUnit.Helpers;
using BudgetAnalyser.Engine.XUnit.TestData;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

// ReSharper disable once InconsistentNaming
public class LedgerBook_GeneralTest
{
    private LedgerBook subject;

    public LedgerBook_GeneralTest()
    {
        this.subject = LedgerBookTestData.TestData1();
    }

    [Fact]
    public void UnlockMostRecentLineShouldNotThrowIfBookIsEmpty()
    {
        this.subject = new LedgerBook { Name = "Foo", Modified = new DateTime(2011, 12, 4, 0, 0, 0, DateTimeKind.Utc), StorageKey = @"C:\TestLedgerBook.xml" };
        var result = this.subject.UnlockMostRecentLine();

        result.ShouldBeNull();
    }

    [Fact]
    public void UnlockMostRecentLineShouldReturnMostRecentLine()
    {
        var result = this.subject.UnlockMostRecentLine();
        var expectedLine = this.subject.Reconciliations.OrderByDescending(e => e.Date).First();

        result.ShouldBeSameAs(expectedLine);
    }

    [Fact]
    public void UnlockMostRecentLineShouldUnlockMostRecentLineAndMarkItIsNew()
    {
        var result = this.subject.UnlockMostRecentLine();

        result!.IsNew.ShouldBeTrue();
    }

    [Fact]
    public void UsingTestData2_Output()
    {
        var book = LedgerBookTestData.TestData2();
        book.Output();
    }

    [Fact]
    public void Validate_LedgerBookMustHaveAFileName()
    {
        this.subject.StorageKey = "";
        var strings = new StringBuilder();
        var valid = this.subject.Validate(strings);
        valid.ShouldBeFalse();
        strings.ToString().Contains("must have a file name").ShouldBeTrue();
    }

    [Fact]
    public void Validate_LedgerBookMustNotOutOfSequenceReconciliations()
    {
        var mostRecentRecon = this.subject.Reconciliations.First();
        var previousRecon = this.subject.Reconciliations.Skip(1).FirstOrDefault();
        previousRecon!.Date = mostRecentRecon.Date.AddDays(14);
        var strings = new StringBuilder();
        var valid = this.subject.Validate(strings);
        valid.ShouldBeFalse();
        strings.ToString().Contains("out of sequence dates").ShouldBeTrue();
    }

    [Fact]
    public void Validate_ShouldBeValid_GivenTestData2()
    {
        this.subject.Validate(new StringBuilder()).ShouldBeTrue();
    }
}
