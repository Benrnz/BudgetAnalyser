using System;
using System.Linq;
using System.Text;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.UnitTest.Helper;
using BudgetAnalyser.Engine.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Ledger;

[TestClass]
// ReSharper disable once InconsistentNaming
public class LedgerBook_GeneralTest
{
    private LedgerBook subject;

    [TestInitialize]
    public void TestInitialise()
    {
        this.subject = LedgerBookTestData.TestData1();
    }

    [TestMethod]
    public void UnlockMostRecentLineShouldNotThrowIfBookIsEmpty()
    {
        this.subject = new LedgerBook
        {
            Name = "Foo",
            Modified = new DateTime(2011, 12, 4),
            StorageKey = @"C:\TestLedgerBook.xml"
        };
        var result = this.subject.UnlockMostRecentLine();

        Assert.IsNull(result);
    }

    [TestMethod]
    public void UnlockMostRecentLineShouldReturnMostRecentLine()
    {
        var result = this.subject.UnlockMostRecentLine();
        var expectedLine = this.subject.Reconciliations.OrderByDescending(e => e.Date).First();

        Assert.AreSame(expectedLine, result);
    }

    [TestMethod]
    public void UnlockMostRecentLineShouldUnlockMostRecentLineAndMarkItIsNew()
    {
        var result = this.subject.UnlockMostRecentLine();

        Assert.IsTrue(result.IsNew);
    }

    [TestMethod]
    public void UsingTestData2_Output()
    {
        var book = LedgerBookTestData.TestData2();
        book.Output();
    }

    [TestMethod]
    public void Validate_LedgerBookMustHaveAFileName()
    {
        this.subject.StorageKey = "";
        var strings = new StringBuilder();
        var valid = this.subject.Validate(strings);
        Assert.IsFalse(valid);
        Assert.IsTrue(strings.ToString().Contains("must have a file name"));
    }

    [TestMethod]
    public void Validate_LedgerBookMustNotOutOfSequenceReconciliations()
    {
        var mostRecentRecon = this.subject.Reconciliations.First();
        var previousRecon = this.subject.Reconciliations.Skip(1).FirstOrDefault();
        previousRecon.Date = mostRecentRecon.Date.AddDays(14);
        var strings = new StringBuilder();
        var valid = this.subject.Validate(strings);
        Assert.IsFalse(valid);
        Assert.IsTrue(strings.ToString().Contains("out of sequence dates"));
    }

    [TestMethod]
    public void Validate_ShouldBeValid_GivenTestData2()
    {
        Assert.IsTrue(this.subject.Validate(new StringBuilder()));
    }
}