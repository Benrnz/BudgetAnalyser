#nullable disable
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.XUnit.TestData;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

public class TransferFundsCommandTest
{
    private TransferFundsCommand subject = null!;

    [Fact]
    public void IsValid_ShouldBeFalse_GivenBlankNarrative()
    {
        this.subject = new TransferFundsCommand
        {
            FromLedger = new SpentPerPeriodLedger
            {
                BudgetBucket = TransactionsListModelTestData.PhoneBucket,
                StoredInAccount = TransactionsListModelTestData.ChequeAccount
            },
            ToLedger = new SavedUpForLedger
            {
                BudgetBucket = TransactionsListModelTestData.InsHomeBucket,
                StoredInAccount = TransactionsListModelTestData.ChequeAccount
            },
            Narrative = "",
            TransferAmount = 0M
        };
    }

    [Fact]
    public void IsValid_ShouldBeFalse_GivenLedgersAreTheBothSurplusForSameAccount()
    {
        this.subject = new TransferFundsCommand
        {
            FromLedger = new SurplusLedger
            {
                BudgetBucket = new SurplusBucket(),
                StoredInAccount = TransactionsListModelTestData.ChequeAccount
            },
            ToLedger = new SurplusLedger
            {
                BudgetBucket = new SurplusBucket(),
                StoredInAccount = TransactionsListModelTestData.ChequeAccount
            },
            Narrative = "Foo",
            TransferAmount = 1
        };

        this.subject.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_ShouldBeFalse_GivenLedgersAreTheSame()
    {
        var ledger = new SavedUpForLedger
        {
            BudgetBucket = TransactionsListModelTestData.InsHomeBucket,
            StoredInAccount = TransactionsListModelTestData.ChequeAccount
        };
        this.subject = new TransferFundsCommand
        {
            FromLedger = ledger,
            ToLedger = ledger,
            Narrative = "Foo",
            TransferAmount = 1
        };

        this.subject.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_ShouldBeFalse_GivenNegativeAmount()
    {
        this.subject = new TransferFundsCommand
        {
            FromLedger = new SpentPerPeriodLedger
            {
                BudgetBucket = TransactionsListModelTestData.PhoneBucket,
                StoredInAccount = TransactionsListModelTestData.ChequeAccount
            },
            ToLedger = new SavedUpForLedger
            {
                BudgetBucket = TransactionsListModelTestData.InsHomeBucket,
                StoredInAccount = TransactionsListModelTestData.ChequeAccount
            },
            Narrative = "Foo",
            TransferAmount = -20M
        };

        this.subject.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_ShouldBeFalse_GivenNullFromLedger()
    {
        this.subject = new TransferFundsCommand
        {
            FromLedger = null,
            ToLedger = new SavedUpForLedger
            {
                BudgetBucket = TransactionsListModelTestData.InsHomeBucket,
                StoredInAccount = TransactionsListModelTestData.ChequeAccount
            },
            Narrative = "Foo",
            TransferAmount = 1
        };

        this.subject.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_ShouldBeFalse_GivenNullNarrative()
    {
        this.subject = new TransferFundsCommand
        {
            FromLedger = new SpentPerPeriodLedger
            {
                BudgetBucket = TransactionsListModelTestData.PhoneBucket,
                StoredInAccount = TransactionsListModelTestData.ChequeAccount
            },
            ToLedger = new SavedUpForLedger
            {
                BudgetBucket = TransactionsListModelTestData.InsHomeBucket,
                StoredInAccount = TransactionsListModelTestData.ChequeAccount
            },
            Narrative = null!,
            TransferAmount = 0M
        };
    }

    [Fact]
    public void IsValid_ShouldBeFalse_GivenNullToLedger()
    {
        this.subject = new TransferFundsCommand
        {
            FromLedger = new SavedUpForLedger
            {
                BudgetBucket = TransactionsListModelTestData.InsHomeBucket,
                StoredInAccount = TransactionsListModelTestData.ChequeAccount
            },
            ToLedger = null,
            Narrative = "Foo",
            TransferAmount = 1
        };

        this.subject.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_ShouldBeFalse_GivenSmallFractionalAmount()
    {
        this.subject = new TransferFundsCommand
        {
            FromLedger = new SpentPerPeriodLedger
            {
                BudgetBucket = TransactionsListModelTestData.PhoneBucket,
                StoredInAccount = TransactionsListModelTestData.ChequeAccount
            },
            ToLedger = new SavedUpForLedger
            {
                BudgetBucket = TransactionsListModelTestData.InsHomeBucket,
                StoredInAccount = TransactionsListModelTestData.ChequeAccount
            },
            Narrative = "Foo",
            TransferAmount = 0.00001M
        };

        this.subject.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_ShouldBeFalse_GivenZeroAmount()
    {
        this.subject = new TransferFundsCommand
        {
            FromLedger = new SpentPerPeriodLedger
            {
                BudgetBucket = TransactionsListModelTestData.PhoneBucket,
                StoredInAccount = TransactionsListModelTestData.ChequeAccount
            },
            ToLedger = new SavedUpForLedger
            {
                BudgetBucket = TransactionsListModelTestData.InsHomeBucket,
                StoredInAccount = TransactionsListModelTestData.ChequeAccount
            },
            Narrative = "Foo",
            TransferAmount = 0M
        };

        this.subject.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_ShouldBeTrue_GivenThisRealExample()
    {
        this.subject = new TransferFundsCommand
        {
            FromLedger = new SurplusLedger
            {
                BudgetBucket = new SurplusBucket(),
                StoredInAccount = TransactionsListModelTestData.ChequeAccount
            },
            ToLedger = new SavedUpForLedger
            {
                BudgetBucket = TransactionsListModelTestData.CarMtcBucket,
                StoredInAccount = TransactionsListModelTestData.ChequeAccount
            },
            Narrative = "Foo",
            TransferAmount = 12
        };

        this.subject.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_ShouldIndicateBankTransferNotRequired_GivenFromAndToLedgersAreSame()
    {
        this.subject = new TransferFundsCommand
        {
            FromLedger = new SpentPerPeriodLedger
            {
                BudgetBucket = TransactionsListModelTestData.PhoneBucket,
                StoredInAccount = TransactionsListModelTestData.ChequeAccount
            },
            ToLedger = new SavedUpForLedger
            {
                BudgetBucket = TransactionsListModelTestData.InsHomeBucket,
                StoredInAccount = TransactionsListModelTestData.ChequeAccount
            },
            Narrative = "Foo",
            TransferAmount = 12.34M
        };

        this.subject.BankTransferRequired.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_ShouldIndicateBankTransferRequired_GivenFromAndToLedgersAreDifferent()
    {
        this.subject = new TransferFundsCommand
        {
            FromLedger = new SpentPerPeriodLedger
            {
                BudgetBucket = TransactionsListModelTestData.PhoneBucket,
                StoredInAccount = TransactionsListModelTestData.ChequeAccount
            },
            ToLedger = new SavedUpForLedger
            {
                BudgetBucket = TransactionsListModelTestData.InsHomeBucket,
                StoredInAccount = TransactionsListModelTestData.SavingsAccount
            },
            Narrative = "Foo",
            TransferAmount = 12.34M
        };

        this.subject.BankTransferRequired.ShouldBeTrue();
    }

    [Fact]
    public void SettingFromLedger_ShouldPopulateAutoMatchingReference_GivenFromAndToLedgersAreDifferent()
    {
        this.subject = new TransferFundsCommand
        {
            FromLedger = new SpentPerPeriodLedger
            {
                BudgetBucket = TransactionsListModelTestData.PhoneBucket,
                StoredInAccount = TransactionsListModelTestData.ChequeAccount
            },
            ToLedger = new SavedUpForLedger
            {
                BudgetBucket = TransactionsListModelTestData.InsHomeBucket,
                StoredInAccount = TransactionsListModelTestData.SavingsAccount
            },
            Narrative = "Foo",
            TransferAmount = 12.34M
        };

        this.subject.AutoMatchingReference.IsSomething().ShouldBeTrue();
    }
}
