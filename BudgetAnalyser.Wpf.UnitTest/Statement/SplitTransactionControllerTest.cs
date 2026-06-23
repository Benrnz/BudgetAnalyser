using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.ShellDialog;
using BudgetAnalyser.Statement;
using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Rees.UnitTestUtilities;

namespace BudgetAnalyser.Wpf.UnitTest.Statement;

[TestClass]
public class SplitTransactionControllerTest
{
    private Mock<IBudgetBucketRepository> mockBucketRepo;
    private Mock<IMessenger> mockMessenger;
    private Mock<IUiContext> mockUiContext;

    [TestMethod]
    public void ChangingSplinterAmount1_MakesValidTrue_WhenAmountsSumToOriginal()
    {
        var subject = CreateSubject();
        var original = new Transaction { Amount = 100M, BudgetBucket = new TestBucket("TB", "Test") };

        // Bypass setters and set private fields to desired splinters, then set OriginalTransaction so Valid evaluates against it
        SetPrivateAmounts(subject, 40M, 60M);
        SetOriginalTransaction(subject, original);

        Assert.AreEqual(40M, subject.SplinterAmount1);
        Assert.AreEqual(60M, subject.SplinterAmount2);
        Assert.IsTrue(subject.Valid);
    }

    [TestMethod]
    public void ChangingSplinterAmount1_ToZero_MakesValidFalse()
    {
        var subject = CreateSubject();
        var original = new Transaction { Amount = 100M, BudgetBucket = new TestBucket("TB", "Test") };

        SetPrivateAmounts(subject, 0M, 100M);
        SetOriginalTransaction(subject, original);

        Assert.AreEqual(0M, subject.SplinterAmount1);
        Assert.AreEqual(100M, subject.SplinterAmount2);
        Assert.IsFalse(subject.Valid);
    }

    [TestMethod]
    public void ChangingSplinterAmount2_MakesValidTrue_WhenAmountsSumToOriginal()
    {
        var subject = CreateSubject();
        var original = new Transaction { Amount = 100M, BudgetBucket = new TestBucket("TB", "Test") };

        SetPrivateAmounts(subject, 75M, 25M);
        SetOriginalTransaction(subject, original);

        Assert.AreEqual(25M, subject.SplinterAmount2);
        Assert.AreEqual(75M, subject.SplinterAmount1);
        Assert.IsTrue(subject.Valid);
    }

    [TestMethod]
    public void ChangingSplinterAmount2_ToZero_MakesValidFalse()
    {
        var subject = CreateSubject();
        var original = new Transaction { Amount = 100M, BudgetBucket = new TestBucket("TB", "Test") };

        SetPrivateAmounts(subject, 100M, 0M);
        SetOriginalTransaction(subject, original);

        Assert.AreEqual(0M, subject.SplinterAmount2);
        Assert.AreEqual(100M, subject.SplinterAmount1);
        Assert.IsFalse(subject.Valid);
    }

    [TestMethod]
    public void NegativeAmounts_SetSplinterAmount1_ToNegative50_IsInvalid()
    {
        var subject = CreateSubject();
        var original = new Transaction { Amount = -100.22M, BudgetBucket = new TestBucket("TB", "Test") };
        subject.ShowDialog(original, Guid.NewGuid());

        subject.SplinterAmount1 = -50M; // direct setter
        Assert.IsFalse(subject.Valid);
    }

    [TestMethod]
    public void NegativeAmounts_Splinter1_0_Splinter2_Neg100Point22_IsInvalid()
    {
        var subject = CreateSubject();
        var original = new Transaction { Amount = -100.22M, BudgetBucket = new TestBucket("TB", "Test") };
        subject.ShowDialog(original, Guid.NewGuid());

        subject.SplinterAmount1 = 0M;
        subject.SplinterAmount2 = -100.22M;
        Assert.IsFalse(subject.Valid);
    }

    [TestMethod]
    public void NegativeAmounts_Splinter1_1_Splinter2_Neg101Point22_IsInvalid()
    {
        var subject = CreateSubject();
        var original = new Transaction { Amount = -100.22M, BudgetBucket = new TestBucket("TB", "Test") };
        subject.ShowDialog(original, Guid.NewGuid());

        subject.SplinterAmount1 = 1M;
        subject.SplinterAmount2 = -101.22M;
        Assert.IsTrue(subject.Valid);
    }

    [TestMethod]
    public void NegativeAmounts_Splinter1_Neg50_Splinter2_Neg50Point22_IsValid()
    {
        var subject = CreateSubject();
        var original = new Transaction { Amount = -100.22M, BudgetBucket = new TestBucket("TB", "Test") };
        subject.ShowDialog(original, Guid.NewGuid());

        subject.SplinterAmount1 = -50M;
        subject.SplinterAmount2 = -50.22M;
        Console.WriteLine("SpliterAmount1: " + subject.SplinterAmount1);
        Console.WriteLine("SpliterAmount2: " + subject.SplinterAmount2);
        Assert.IsTrue(subject.Valid);
    }

    [TestMethod]
    public void OnShellDialogResponseReceived_MessageWithOkOrSave_ValidRemainsTrue_WhenNotForMe()
    {
        var subject = CreateSubject();
        var original = new Transaction { Amount = 100M, BudgetBucket = new TestBucket("TB", "Test") };

        // Set the amounts and original via reflection so we control the values
        SetPrivateAmounts(subject, 40M, 60M);
        SetOriginalTransaction(subject, original);

        Assert.IsTrue(subject.Valid);

        // Send an Ok response that is NOT for this dialog (CorrelationId empty)
        var messageOk = new ShellDialogResponseMessage(subject, ShellDialogButton.Ok) { CorrelationId = Guid.Empty };
        this.mockMessenger.Object.Send(messageOk);
        Assert.IsTrue(subject.Valid);

        // Send a Save response that is NOT for this dialog
        var messageSave = new ShellDialogResponseMessage(subject, ShellDialogButton.Save) { CorrelationId = Guid.Empty };
        this.mockMessenger.Object.Send(messageSave);
        Assert.IsTrue(subject.Valid);
    }

    [TestInitialize]
    public void TestInitialise()
    {
        this.mockMessenger = new Mock<IMessenger>();
        this.mockUiContext = new Mock<IUiContext>();
        this.mockUiContext.Setup(m => m.Messenger).Returns(this.mockMessenger.Object);
        this.mockUiContext.Setup(m => m.UserPrompts).Returns(() => null!);

        this.mockBucketRepo = new Mock<IBudgetBucketRepository>();
        this.mockBucketRepo.Setup(r => r.Buckets).Returns(Array.Empty<BudgetBucket>());
    }

    [TestMethod]
    public void Valid_IsFalse_WhenOriginalTransactionIsNullOrAmountZero()
    {
        var subject = CreateSubject();
        // No original set -> OriginalTransaction is null
        Assert.IsFalse(subject.Valid);

        // Original transaction amount zero
        var originalZero = new Transaction { Amount = 0M, BudgetBucket = new TestBucket("TB", "Test") };
        SetPrivateAmounts(subject, 0M, 0M);
        SetOriginalTransaction(subject, originalZero);
        Assert.IsFalse(subject.Valid);
    }

    private SplitTransactionController CreateSubject()
    {
        // Ensure the mock UiContext returns the mock messenger when the controller is constructed.
        this.mockUiContext.Setup(m => m.Messenger).Returns(this.mockMessenger.Object);
        return new SplitTransactionController(this.mockUiContext.Object, this.mockBucketRepo.Object);
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
        public TestBucket(string code, string name) : base(code, name) { }
    }
}
