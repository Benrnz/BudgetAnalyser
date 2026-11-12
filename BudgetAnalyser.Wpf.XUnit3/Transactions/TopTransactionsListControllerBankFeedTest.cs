using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.BankFeed;
using BudgetAnalyser.Engine.Transactions;
using NSubstitute;
using Rees.UnitTestUtilities;
using Shouldly;

namespace BudgetAnalyser.Wpf.XUnit3.Transactions;

/// <summary>
///     Tests the "Download from bank feed" button on the Transactions tab. Uses the existing <see cref="TopTransactionsListControllerTest.CreateContext" /> and reaches its
///     bank feed configuration fake through the controller's private field.
/// </summary>
public class TopTransactionsListControllerBankFeedTest
{
    [Fact]
    public void DownloadBankFeed_ShouldBeDisabled_GivenNoTransactionsLoaded()
    {
        var (context, configuration) = CreateContext();
        configuration.IsConfigured.Returns(true);
        context.Subject.ViewModel.TransactionsList = null;

        context.Subject.DownloadBankFeedCommand.CanExecute(null).ShouldBeFalse();
    }

    [Fact]
    public void DownloadBankFeed_ShouldBeDisabled_GivenSettingsIncomplete()
    {
        var (context, configuration) = CreateContext();
        configuration.IsConfigured.Returns(false);
        context.Subject.ViewModel.TransactionsList = new TransactionsListModel(Substitute.For<ILogger>());

        context.Subject.DownloadBankFeedCommand.CanExecute(null).ShouldBeFalse();
        context.Subject.DownloadBankFeedToolTip.ShouldBe("Set up the bank feed API on the Settings tab");
    }

    [Fact]
    public void DownloadBankFeed_ShouldBeEnabled_GivenConfiguredAndTransactionsLoaded()
    {
        var (context, configuration) = CreateContext();
        configuration.IsConfigured.Returns(true);
        context.Subject.ViewModel.TransactionsList = new TransactionsListModel(Substitute.For<ILogger>());

        context.Subject.DownloadBankFeedCommand.CanExecute(null).ShouldBeTrue();
        context.Subject.DownloadBankFeedToolTip.ShouldStartWith("Download and merge in new transactions from the bank feed API. This will also save any unsaved changes first.");
    }

    [Fact]
    public void DownloadBankFeed_ShouldRequery_GivenConfigurationChanged()
    {
        var (context, configuration) = CreateContext();
        var requeried = 0;
        var tooltipChanged = 0;
        context.Subject.DownloadBankFeedCommand.CanExecuteChanged += (_, _) => requeried++;
        context.Subject.PropertyChanged += (_, e) => tooltipChanged += e.PropertyName == nameof(context.Subject.DownloadBankFeedToolTip) ? 1 : 0;

        configuration.ConfigurationChanged += Raise.Event();

        requeried.ShouldBe(1);
        tooltipChanged.ShouldBe(1);
    }

    [Fact]
    public void DownloadBankFeed_ShouldRequery_GivenTransactionsListChanged()
    {
        var (context, _) = CreateContext();
        var requeried = 0;
        context.Subject.DownloadBankFeedCommand.CanExecuteChanged += (_, _) => requeried++;

        context.Subject.ViewModel.TransactionsList = new TransactionsListModel(Substitute.For<ILogger>());

        requeried.ShouldBeGreaterThan(0);
    }

    private static (TopTransactionsListControllerTest.TestContext Context, IBankFeedConfigurationService Configuration) CreateContext()
    {
        var context = TopTransactionsListControllerTest.CreateContext();
        var configuration = (IBankFeedConfigurationService)PrivateAccessor.GetField(context.Subject, "bankFeedConfiguration")!;
        return (context, configuration);
    }
}
