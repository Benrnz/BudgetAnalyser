#nullable disable
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Reconciliation;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

// ReSharper disable once InconsistentNaming
public class ReconciliationManagerTest_TransferFunds
{
    private readonly LedgerBucket insHomeSavLedger;
    private readonly IBudgetBucketRepository mockBucketRepo;
    private readonly IReconciliationBuilder mockReconciliationBuilder;
    private readonly IReconciliationConsistency mockReconciliationConsistency;
    private readonly ITransactionRuleService mockRuleService;
    private readonly LedgerBucket phNetChqLedger;
    private readonly ReconciliationCreationManager subject;
    private readonly LedgerBucket surplusChqLedger;
    private readonly LedgerEntryLine testDataEntryLine;
    private readonly LedgerBook testDataLedgerBook;

    public ReconciliationManagerTest_TransferFunds()
    {
        this.mockBucketRepo = Substitute.For<IBudgetBucketRepository>();
        this.mockRuleService = Substitute.For<ITransactionRuleService>();
        this.mockReconciliationConsistency = Substitute.For<IReconciliationConsistency>();
        this.mockReconciliationBuilder = Substitute.For<IReconciliationBuilder>();
        this.subject = new ReconciliationCreationManager(this.mockRuleService, this.mockReconciliationConsistency, this.mockReconciliationBuilder, new FakeLogger());

        this.testDataLedgerBook = LedgerBookTestData.TestData5(recons => new LedgerBookTestHarness(recons) { StorageKey = "Test Ledger Book.xaml" });
        this.testDataEntryLine = this.testDataLedgerBook.Reconciliations.First();
        this.testDataEntryLine.Unlock();

        this.surplusChqLedger = new SurplusLedger { StoredInAccount = TransactionsListModelTestData.ChequeAccount, BudgetBucket = TransactionsListModelTestData.SurplusBucket };
        this.insHomeSavLedger = this.testDataLedgerBook.Ledgers.Single(l => l.BudgetBucket == TransactionsListModelTestData.InsHomeBucket);
        this.phNetChqLedger = this.testDataLedgerBook.Ledgers.Single(l => l.BudgetBucket == TransactionsListModelTestData.PhoneBucket);
    }

    [Fact]
    public void TransferFunds_ShouldCreateAutoMatchingRule_GivenTransferFromChqSurplusToSavingsInsHome()
    {
        var transferFundsData = new TransferFundsCommand
        {
            AutoMatchingReference = "FooTest12345",
            BankTransferRequired = true,
            FromLedger = LedgerBookTestData.SurplusLedger,
            Narrative = "Save excess for November",
            ToLedger = LedgerBookTestData.HouseInsLedgerSavingsAccount,
            TransferAmount = 200M
        };

        this.mockRuleService.CreateNewSingleUseRule(transferFundsData.FromLedger.BudgetBucket.Code, Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<string>(), -200M, true)
            .Returns(new SingleUseMatchingRule(this.mockBucketRepo));
        this.mockRuleService.CreateNewSingleUseRule(transferFundsData.ToLedger.BudgetBucket.Code, Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<string>(), 200M, true)
            .Returns(new SingleUseMatchingRule(this.mockBucketRepo));

        this.subject.TransferFunds(this.testDataLedgerBook, transferFundsData, this.testDataEntryLine);
        this.mockRuleService.Received(1).CreateNewSingleUseRule(transferFundsData.FromLedger.BudgetBucket.Code, Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<string>(), -200M, true);
        this.mockRuleService.Received(1).CreateNewSingleUseRule(transferFundsData.ToLedger.BudgetBucket.Code, Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<string>(), 200M, true);
    }

    [Fact]
    public void TransferFunds_ShouldDecreaseChqBalance_GivenTransferFromChqSurplusToSavingsInsHome()
    {
        var transferDetails = new TransferFundsCommand { FromLedger = this.surplusChqLedger, ToLedger = this.insHomeSavLedger, TransferAmount = 22.00M, Narrative = "Testing 123" };

        var beforeBalance = this.testDataEntryLine.BankBalances.First(b => b.Account == TransactionsListModelTestData.ChequeAccount).Balance
                            + this.testDataEntryLine.BankBalanceAdjustments
                                .Where(a => a.BankAccount == TransactionsListModelTestData.ChequeAccount)
                                .Sum(a => a.Amount);
        this.subject.TransferFunds(this.testDataLedgerBook, transferDetails, this.testDataEntryLine);
        var afterBalance = this.testDataEntryLine.BankBalances.First(b => b.Account == TransactionsListModelTestData.ChequeAccount).Balance
                           + this.testDataEntryLine.BankBalanceAdjustments
                               .Where(a => a.BankAccount == TransactionsListModelTestData.ChequeAccount)
                               .Sum(a => a.Amount);

        afterBalance.ShouldBe(beforeBalance - transferDetails.TransferAmount);
    }

    [Fact]
    public void TransferFunds_ShouldDecreaseChqSurplus_GivenTransferFromChqSurplusToChqCarMtc()
    {
        var transferDetails = new TransferFundsCommand { FromLedger = this.surplusChqLedger, ToLedger = this.phNetChqLedger, TransferAmount = 22.00M, Narrative = "Testing 123" };

        var beforeBalance = this.testDataEntryLine.SurplusBalances.First(b => b.Account == TransactionsListModelTestData.ChequeAccount).Balance;
        this.subject.TransferFunds(this.testDataLedgerBook, transferDetails, this.testDataEntryLine);
        var afterBalance = this.testDataEntryLine.SurplusBalances.First(b => b.Account == TransactionsListModelTestData.ChequeAccount).Balance;

        afterBalance.ShouldBe(beforeBalance - transferDetails.TransferAmount);
    }

    [Fact]
    public void TransferFunds_ShouldDecreaseChqSurplus_GivenTransferFromChqSurplusToSavingsInsHome()
    {
        var transferDetails = new TransferFundsCommand { FromLedger = this.surplusChqLedger, ToLedger = this.insHomeSavLedger, TransferAmount = 22.00M, Narrative = "Testing 123" };

        var beforeBalance = this.testDataEntryLine.SurplusBalances.First(b => b.Account == TransactionsListModelTestData.ChequeAccount).Balance;
        this.subject.TransferFunds(this.testDataLedgerBook, transferDetails, this.testDataEntryLine);
        var afterBalance = this.testDataEntryLine.SurplusBalances.First(b => b.Account == TransactionsListModelTestData.ChequeAccount).Balance;

        afterBalance.ShouldBe(beforeBalance - transferDetails.TransferAmount);
    }

    [Fact]
    public void TransferFunds_ShouldIncreaseChqCarMtc_GivenTransferFromChqSurplusToChqCarMtc()
    {
        var transferDetails = new TransferFundsCommand { FromLedger = this.surplusChqLedger, ToLedger = this.phNetChqLedger, TransferAmount = 22.00M, Narrative = "Testing 123" };

        var beforeBalance = this.testDataEntryLine.Entries.First(e => e.LedgerBucket == this.phNetChqLedger).Balance;
        this.subject.TransferFunds(this.testDataLedgerBook, transferDetails, this.testDataEntryLine);
        var afterBalance = this.testDataEntryLine.Entries.First(e => e.LedgerBucket == this.phNetChqLedger).Balance;

        afterBalance.ShouldBe(beforeBalance + transferDetails.TransferAmount);
    }

    [Fact]
    public void TransferFunds_ShouldIncreaseSavBalance_GivenTransferFromChqSurplusToSavingsInsHome()
    {
        var transferDetails = new TransferFundsCommand { FromLedger = this.surplusChqLedger, ToLedger = this.insHomeSavLedger, TransferAmount = 22.00M, Narrative = "Testing 123" };

        var beforeBalance = this.testDataEntryLine.BankBalances.First(b => b.Account == TransactionsListModelTestData.SavingsAccount).Balance
                            + this.testDataEntryLine.BankBalanceAdjustments
                                .Where(a => a.BankAccount == TransactionsListModelTestData.SavingsAccount)
                                .Sum(a => a.Amount);
        this.subject.TransferFunds(this.testDataLedgerBook, transferDetails, this.testDataEntryLine);
        var afterBalance = this.testDataEntryLine.BankBalances.First(b => b.Account == TransactionsListModelTestData.SavingsAccount).Balance
                           + this.testDataEntryLine.BankBalanceAdjustments
                               .Where(a => a.BankAccount == TransactionsListModelTestData.SavingsAccount)
                               .Sum(a => a.Amount);

        afterBalance.ShouldBe(beforeBalance + transferDetails.TransferAmount);
    }

    [Fact]
    public void TransferFunds_ShouldIncreaseSavInsHome_GivenTransferFromChqSurplusToSavingsInsHome()
    {
        var transferDetails = new TransferFundsCommand { FromLedger = this.surplusChqLedger, ToLedger = this.insHomeSavLedger, TransferAmount = 22.00M, Narrative = "Testing 123" };

        var beforeBalance = this.testDataEntryLine.Entries.First(e => e.LedgerBucket == this.insHomeSavLedger).Balance;
        this.subject.TransferFunds(this.testDataLedgerBook, transferDetails, this.testDataEntryLine);
        var afterBalance = this.testDataEntryLine.Entries.First(e => e.LedgerBucket == this.insHomeSavLedger).Balance;

        afterBalance.ShouldBe(beforeBalance + transferDetails.TransferAmount);
    }

    [Fact]
    public void TransferFunds_ShouldNotCreateAutoMatchingRule_GivenTransferFromChqSurplusToChqHairCut()
    {
        var transferFundsData = new TransferFundsCommand
        {
            AutoMatchingReference = "FooTest12345",
            BankTransferRequired = false,
            FromLedger = LedgerBookTestData.SurplusLedger,
            Narrative = "Save excess for November",
            ToLedger = LedgerBookTestData.HairLedger,
            TransferAmount = 400M
        };

        var success = true;
        this.mockRuleService.CreateNewSingleUseRule(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<string>(), Arg.Any<decimal?>(), true)
            .Returns(new SingleUseMatchingRule(this.mockBucketRepo)).AndDoes(_ => success = false);

        this.subject.TransferFunds(this.testDataLedgerBook, transferFundsData, this.testDataEntryLine);

        success.ShouldBeTrue();
    }


    [Fact]
    public void TransferFunds_ShouldNotDecreaseChqBalance_GivenTransferFromChqSurplusToChqCarMtc()
    {
        var transferDetails = new TransferFundsCommand { FromLedger = this.surplusChqLedger, ToLedger = this.phNetChqLedger, TransferAmount = 22.00M, Narrative = "Testing 123" };

        var beforeBalance = this.testDataEntryLine.BankBalances.First(b => b.Account == TransactionsListModelTestData.ChequeAccount).Balance
                            + this.testDataEntryLine.BankBalanceAdjustments
                                .Where(a => a.BankAccount == TransactionsListModelTestData.ChequeAccount)
                                .Sum(a => a.Amount);
        this.subject.TransferFunds(this.testDataLedgerBook, transferDetails, this.testDataEntryLine);
        var afterBalance = this.testDataEntryLine.BankBalances.First(b => b.Account == TransactionsListModelTestData.ChequeAccount).Balance
                           + this.testDataEntryLine.BankBalanceAdjustments
                               .Where(a => a.BankAccount == TransactionsListModelTestData.ChequeAccount)
                               .Sum(a => a.Amount);

        afterBalance.ShouldBe(beforeBalance);
    }

    [Fact]
    public void TransferFunds_ShouldNotIncreaseSavBalance_GivenTransferFromChqSurplusToChqCarMtc()
    {
        var transferDetails = new TransferFundsCommand { FromLedger = this.surplusChqLedger, ToLedger = this.phNetChqLedger, TransferAmount = 22.00M, Narrative = "Testing 123" };

        var beforeBalance = this.testDataEntryLine.BankBalances.First(b => b.Account == TransactionsListModelTestData.SavingsAccount).Balance
                            + this.testDataEntryLine.BankBalanceAdjustments
                                .Where(a => a.BankAccount == TransactionsListModelTestData.SavingsAccount)
                                .Sum(a => a.Amount);
        this.subject.TransferFunds(this.testDataLedgerBook, transferDetails, this.testDataEntryLine);
        var afterBalance = this.testDataEntryLine.BankBalances.First(b => b.Account == TransactionsListModelTestData.SavingsAccount).Balance
                           + this.testDataEntryLine.BankBalanceAdjustments
                               .Where(a => a.BankAccount == TransactionsListModelTestData.SavingsAccount)
                               .Sum(a => a.Amount);

        afterBalance.ShouldBe(beforeBalance);
    }


    [Fact]
    public void TransferFunds_ShouldThrow_GivenInvalidTransferDetails()
    {
        Should.Throw<InvalidOperationException>(() => this.subject.TransferFunds(this.testDataLedgerBook, new TransferFundsCommand(), new LedgerEntryLine()));
    }

    [Fact]
    public void TransferFunds_ShouldThrow_GivenNullLedgerEntryLine()
    {
        Should.Throw<ArgumentNullException>(() => this.subject.TransferFunds(this.testDataLedgerBook, new TransferFundsCommand(), null));
    }

    [Fact]
    public void TransferFunds_ShouldThrow_GivenNullTransferDetails()
    {
        Should.Throw<ArgumentNullException>(() => this.subject.TransferFunds(this.testDataLedgerBook, null, new LedgerEntryLine()));
    }
}
