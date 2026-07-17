using System;
using System.ComponentModel;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.ShellDialog;
using BudgetAnalyser.Transactions;
using CommunityToolkit.Mvvm.Messaging;
using NSubstitute;
using Rees.UnitTestUtilities;
using Shouldly;

namespace BudgetAnalyser.Wpf.XUnit3.Transactions;

public class SplitTransactionControllerTest
{
    private readonly IBudgetBucketRepository mockBucketRepo;
    private readonly ITransactionsControllerFileOperations mockFileOperations;
    private readonly IMessenger mockMessenger;
    private readonly ITransactionManagerService mockTransactionsService;

    public SplitTransactionControllerTest()
    {
        this.mockMessenger = Substitute.For<IMessenger>();
        this.mockBucketRepo = Substitute.For<IBudgetBucketRepository>();
        this.mockBucketRepo.Buckets.Returns(Array.Empty<BudgetBucket>());
        this.mockTransactionsService = Substitute.For<ITransactionManagerService>();
        this.mockFileOperations = Substitute.For<ITransactionsControllerFileOperations>();
    }

    [Fact]
    public void ChangingSplinterAmount1_MakesValidTrue_WhenAmountsSumToOriginal()
    {
        var subject = CreateSubject();
        subject.SplinterBucket1 = new TestBucket("TB", "Test");
        subject.SplinterBucket2 = new TestBucket("TB", "Test");
        var original = new Transaction { Amount = 100M, BudgetBucket = new TestBucket("TB", "Test") };

        SetPrivateAmounts(subject, 40M, 60M);
        SetOriginalTransaction(subject, original);

        subject.SplinterAmount1.ShouldBe(40M);
        subject.SplinterAmount2.ShouldBe(60M);
        subject.Valid.ShouldBeTrue();
    }

    [Fact]
    public void ChangingSplinterAmount1_ToZero_MakesValidFalse()
    {
        var subject = CreateSubject();
        var original = new Transaction { Amount = 100M, BudgetBucket = new TestBucket("TB", "Test") };

        SetPrivateAmounts(subject, 0M, 100M);
        SetOriginalTransaction(subject, original);

        subject.SplinterAmount1.ShouldBe(0M);
        subject.SplinterAmount2.ShouldBe(100M);
        subject.Valid.ShouldBeFalse();
    }

    [Fact]
    public void ChangingSplinterAmount2_MakesValidTrue_WhenAmountsSumToOriginal()
    {
        var subject = CreateSubject();
        subject.SplinterBucket1 = new TestBucket("TB", "Test");
        subject.SplinterBucket2 = new TestBucket("TB", "Test");

        var original = new Transaction { Amount = 100M, BudgetBucket = new TestBucket("TB", "Test") };

        SetPrivateAmounts(subject, 75M, 25M);
        SetOriginalTransaction(subject, original);

        subject.SplinterAmount2.ShouldBe(25M);
        subject.SplinterAmount1.ShouldBe(75M);
        subject.Valid.ShouldBeTrue();
    }

    [Fact]
    public void ChangingSplinterAmount2_ToZero_MakesValidFalse()
    {
        var subject = CreateSubject();
        var original = new Transaction { Amount = 100M, BudgetBucket = new TestBucket("TB", "Test") };

        SetPrivateAmounts(subject, 100M, 0M);
        SetOriginalTransaction(subject, original);

        subject.SplinterAmount2.ShouldBe(0M);
        subject.SplinterAmount1.ShouldBe(100M);
        subject.Valid.ShouldBeFalse();
    }

    [Fact]
    public void NegativeAmounts_SetSplinterAmount1_ToNegative50_IsInvalid()
    {
        var subject = CreateSubject();
        var original = new Transaction { Amount = -100.22M, BudgetBucket = new TestBucket("TB", "Test") };
        subject.ShowDialog(original);

        subject.SplinterAmount1 = -50M;
        subject.Valid.ShouldBeFalse();
    }

    [Fact]
    public void NegativeAmounts_Splinter1_0_Splinter2_Neg100Point22_IsInvalid()
    {
        var subject = CreateSubject();
        var original = new Transaction { Amount = -100.22M, BudgetBucket = new TestBucket("TB", "Test") };
        subject.ShowDialog(original);

        subject.SplinterAmount1 = 0M;
        subject.SplinterAmount2 = -100.22M;
        subject.Valid.ShouldBeFalse();
    }

    [Fact]
    [Description("Can't mix negative and positive amounts")]
    public void NegativeAmounts_Splinter1_1_Splinter2_Neg101Point22_IsInvalid()
    {
        var subject = CreateSubject();
        var original = new Transaction { Amount = -100.22M, BudgetBucket = new TestBucket("TB", "Test") };
        subject.ShowDialog(original);

        subject.SplinterAmount1 = 1M;
        subject.SplinterAmount2 = -101.22M;
        subject.Valid.ShouldBeFalse();
    }

    [Fact]
    public void NegativeAmounts_Splinter1_Neg50_Splinter2_Neg50Point22_IsValid()
    {
        var subject = CreateSubject();
        var original = new Transaction { Amount = -100.22M, BudgetBucket = new TestBucket("TB", "Test") };
        subject.ShowDialog(original);

        subject.SplinterAmount1 = -50M;
        subject.SplinterAmount2 = -50.22M;
        Console.WriteLine("SpliterAmount1: " + subject.SplinterAmount1);
        Console.WriteLine("SpliterAmount2: " + subject.SplinterAmount2);
        subject.Valid.ShouldBeTrue();
    }

    [Fact]
    public void OnShellDialogResponseReceived_MessageWithOkOrSave_ValidRemainsTrue_WhenNotForMe()
    {
        var subject = CreateSubject();
        subject.SplinterBucket1 = new TestBucket("TB", "Test");
        subject.SplinterBucket2 = new TestBucket("TB", "Test");

        var original = new Transaction { Amount = 100M, BudgetBucket = new TestBucket("TB", "Test") };

        SetPrivateAmounts(subject, 40M, 60M);
        SetOriginalTransaction(subject, original);

        subject.Valid.ShouldBeTrue();

        var messageOk = new ShellDialogResponseMessage(subject, ShellDialogButton.Ok) { CorrelationId = Guid.Empty };
        this.mockMessenger.Send(messageOk);
        subject.Valid.ShouldBeTrue();

        var messageSave = new ShellDialogResponseMessage(subject, ShellDialogButton.Save) { CorrelationId = Guid.Empty };
        this.mockMessenger.Send(messageSave);
        subject.Valid.ShouldBeTrue();
    }

    [Fact]
    public void Valid_IsFalse_WhenOriginalTransactionIsNullOrAmountZero()
    {
        var subject = CreateSubject();
        subject.Valid.ShouldBeFalse();

        var originalZero = new Transaction { Amount = 0M, BudgetBucket = new TestBucket("TB", "Test") };
        SetPrivateAmounts(subject, 0M, 0M);
        SetOriginalTransaction(subject, originalZero);
        subject.Valid.ShouldBeFalse();
    }

    private SplitTransactionController CreateSubject()
    {
        return new SplitTransactionController(this.mockMessenger, this.mockBucketRepo, new FakeLogger(), this.mockTransactionsService, this.mockFileOperations);
    }

    private static void SetOriginalTransaction(SplitTransactionController subject, Transaction tx)
    {
        PrivateAccessor.SetProperty(subject, "OriginalTransaction", tx);
    }

    private static void SetPrivateAmounts(SplitTransactionController subject, decimal a1, decimal a2)
    {
        PrivateAccessor.SetField(subject, "doNotUseSplinterAmount1", a1);
        PrivateAccessor.SetField(subject, "doNotUseSplinterAmount2", a2);
    }

    private class TestBucket : BudgetBucket
    {
        public TestBucket(string code, string name) : base(code, name)
        {
        }
    }
}
