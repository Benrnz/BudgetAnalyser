#nullable disable
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.XUnit.Helpers;
using BudgetAnalyser.Engine.XUnit.TestData;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

// ReSharper disable InconsistentNaming
public class LedgerBook_DataTest
{
    [Fact]
    public void UsingTestData1_FirstLineBankBalanceEqualsLedgerBalance()
    {
        var result = ArrangeAndAct();
        var subject = result.Reconciliations.First();
        subject.TotalBankBalance.ShouldBe(subject.LedgerBalance);
    }

    [Fact]
    public void UsingTestData1_FirstLineHairEntryShouldHaveBalance120()
    {
        var result = ArrangeAndAct();
        var subject = result.Reconciliations.First().Entries.First(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode);
        subject.Balance.ShouldBe(120);
    }

    [Fact]
    public void UsingTestData1_FirstLineHasNoAdjustments()
    {
        var result = ArrangeAndAct();
        result.Reconciliations.First().BankBalanceAdjustments.Count().ShouldBe(0);
    }

    [Fact]
    public void UsingTestData1_FirstLineShouldHave3Entries()
    {
        var result = ArrangeAndAct();
        var subject = result.Reconciliations.First();
        subject.Entries.Count().ShouldBe(3);
    }

    [Fact]
    public void UsingTestData1_FirstLineShouldHaveBankBalance2950()
    {
        var result = ArrangeAndAct();
        var subject = result.Reconciliations.First();
        subject.TotalBankBalance.ShouldBe(2950);
    }

    [Fact]
    public void UsingTestData1_FirstLineShouldHaveDate20130815()
    {
        var result = ArrangeAndAct();
        var subject = result.Reconciliations.First();
        subject.Date.ShouldBe(new DateOnly(2013, 08, 15));
    }

    [Fact]
    public void UsingTestData1_FirstLineShouldHaveSurplusOf2712()
    {
        var result = ArrangeAndAct();
        var subject = result.Reconciliations.First();
        subject.CalculatedSurplus.ShouldBe(2712.97M);
    }

    [Fact]
    public void UsingTestData1_LedgerCountShouldBe3()
    {
        var result = ArrangeAndAct();
        result.Ledgers.Count().ShouldBe(3);
    }

    [Fact]
    public void UsingTestData1_OutputDataInTabularForm()
    {
        var result = ArrangeAndAct();
        result.Output();
    }

    [Fact]
    public void UsingTestData1_ShouldBeHairLedger()
    {
        var result = ArrangeAndAct();
        result.Ledgers.FirstOrDefault(l => l.BudgetBucket.Code == TestDataConstants.HairBucketCode).ShouldNotBeNull();
    }

    [Fact]
    public void UsingTestData1_ShouldBePhoneLedger()
    {
        var result = ArrangeAndAct();
        result.Ledgers.FirstOrDefault(l => l.BudgetBucket.Code == TestDataConstants.PhoneBucketCode).ShouldNotBeNull();
    }

    [Fact]
    public void UsingTestData1_ShouldBePowerLedger()
    {
        var result = ArrangeAndAct();
        result.Ledgers.FirstOrDefault(l => l.BudgetBucket.Code == TestDataConstants.PowerBucketCode).ShouldNotBeNull();
    }

    [Fact]
    public void UsingTestData1_ShouldHave3Lines()
    {
        var result = ArrangeAndAct();
        result.Reconciliations.Count().ShouldBe(3);
    }

    private LedgerBook ArrangeAndAct()
    {
        return LedgerBookTestData.TestData1();
    }
}

// ReSharper restore InconsistentNaming
