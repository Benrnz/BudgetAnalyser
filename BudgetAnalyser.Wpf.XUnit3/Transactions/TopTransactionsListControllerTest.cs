#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.Matching;
using BudgetAnalyser.Transactions;
using CommunityToolkit.Mvvm.Messaging;
using NSubstitute;
using Rees.UnitTestUtilities;
using Rees.Wpf.Contracts;
using Shouldly;

namespace BudgetAnalyser.Wpf.XUnit3.Transactions;

public class TopTransactionsListControllerTest
{
    [Fact]
    public void PageSize_ShouldDefaultToTen_WhenSetToZero()
    {
        var context = CreateContext();

        context.Subject.PageSize = 0;

        context.Subject.PageSize.ShouldBe(10);
    }

    [Fact]
    public void CurrentPage_ShouldUpdatePagedTransactionsAndNavigationFlags()
    {
        var context = CreateContext();
        var transactions = CreateTransactions(25);
        PrivateAccessor.SetProperty(context.Subject.ViewModel, "Transactions", transactions);
        context.Subject.PageSize = 10;

        context.Subject.CurrentPage = 2;

        context.Subject.ViewModel.PagedTransactions.Count.ShouldBe(10);
        context.Subject.ViewModel.PagedTransactions.First().ShouldBeSameAs(transactions[10]);
        context.Subject.CanNavigateNext.ShouldBeTrue();
        context.Subject.CanNavigatePrevious.ShouldBeTrue();
    }

    [Fact]
    public void NavigateNextPage_ShouldNotAdvancePastLastPage()
    {
        var context = CreateContext();
        PrivateAccessor.SetProperty(context.Subject.ViewModel, "Transactions", CreateTransactions(15));
        context.Subject.PageSize = 10;
        context.Subject.CurrentPage = 1;

        context.Subject.NavigateNextPage();
        context.Subject.NavigateNextPage();

        context.Subject.CurrentPage.ShouldBe(2);
    }

    [Fact]
    public void NavigatePreviousPage_ShouldNotGoBelowFirstPage()
    {
        var context = CreateContext();
        context.Subject.CurrentPage = 1;

        context.Subject.NavigatePreviousPage();

        context.Subject.CurrentPage.ShouldBe(1);
    }

    [Fact]
    public void TextFilter_ShouldFilterBySearchText_AndResetCurrentPage()
    {
        var context = CreateContext();
        var filtered = CreateTransactions(3);
        context.TransactionService.FilterBySearchText("pay").Returns(filtered);
        context.Subject.CurrentPage = 2;

        context.Subject.TextFilter = "pay";

        context.TransactionService.Received(1).FilterBySearchText("pay");
        context.Subject.CurrentPage.ShouldBe(1);
        context.Subject.ViewModel.Transactions.ShouldBeSameAs(filtered);
    }

    [Fact]
    public void BucketFilter_ShouldFilterByBucket_AndResetCurrentPage()
    {
        var context = CreateContext();
        var filtered = CreateTransactions(2);
        context.TransactionService.FilterByBucket("FOOD").Returns(filtered);
        context.Subject.CurrentPage = 2;

        context.Subject.BucketFilter = "FOOD";

        context.TransactionService.Received(1).FilterByBucket("FOOD");
        context.Subject.CurrentPage.ShouldBe(1);
        context.Subject.ViewModel.Transactions.ShouldBeSameAs(filtered);
    }

    [Fact]
    public void ClearSearch_ShouldClearFilter_AndRestoreUnfilteredTransactions()
    {
        var context = CreateContext();
        var allTransactions = CreateTransactions(4);
        context.TransactionService.ClearBucketAndTextFilters().Returns(allTransactions);
        context.Subject.TextFilter = "rent";

        context.Subject.ClearSearch();

        context.Subject.TextFilter.ShouldBeNull();
        context.TransactionService.Received(1).ClearBucketAndTextFilters();
        context.Subject.ViewModel.Transactions.ShouldBeSameAs(allTransactions);
    }

    [Fact]
    public void FilterAppliedMessage_ShouldRefreshTransactions_WhenSentByAnotherController()
    {
        var context = CreateContext();
        var criteria = new GlobalFilterCriteria();
        var transactionsListModel = new TransactionsListModel(Substitute.For<ILogger>());
        var filtered = CreateTransactions(2);
        PrivateAccessor.SetProperty(transactionsListModel, "Transactions", filtered);
        context.TransactionService.TransactionsListModel.Returns(transactionsListModel);
        context.TransactionService.ClearBucketAndTextFilters().Returns(filtered);
        context.TransactionService.FilterByBucket(Arg.Any<string?>()).Returns(filtered);
        context.Subject.ViewModel.TransactionsList = transactionsListModel;
        context.Subject.BucketFilter = "FOOD";
        context.Subject.TextFilter = "search-text";
        context.Subject.CurrentPage = 2;

        context.Messenger.Send(new FilterAppliedMessage(new object(), criteria));

        context.TransactionService.Received(1).FilterTransactions(criteria);
        context.Subject.TextFilter.ShouldBeNull();
        context.Subject.BucketFilter.ShouldBe("FOOD");
        context.Subject.CurrentPage.ShouldBe(1);
    }

    private static List<Transaction> CreateTransactions(int count)
    {
        return Enumerable.Range(1, count).Select(
            i => new Transaction
            {
                Date = new DateOnly(2026, 1, 1).AddDays(i),
                Amount = i,
                Description = "Tx " + i
            }).ToList();
    }

    private static TestContext CreateContext()
    {
        var messenger = new WeakReferenceMessenger();
        var messageBox = Substitute.For<IUserMessageBox>();
        var yesNoBox = Substitute.For<IUserQuestionBoxYesNo>();
        var inputBox = Substitute.For<IUserInputBox>();
        var transactionService = Substitute.For<ITransactionManagerService>();
        var applicationDatabaseFacade = Substitute.For<IApplicationDatabaseFacade>();
        var accountTypeRepository = Substitute.For<IAccountTypeRepository>();
        var bucketRepository = Substitute.For<IBudgetBucketRepository>();
        var transactionRuleService = Substitute.For<ITransactionRuleService>();
        var logger = Substitute.For<ILogger>();

        bucketRepository.Buckets.Returns(Array.Empty<BudgetBucket>());
        transactionService.ClearBucketAndTextFilters().Returns(new List<Transaction>());
        transactionService.FilterBySearchText(Arg.Any<string?>()).Returns(new List<Transaction>());
        transactionService.FilterByBucket(Arg.Any<string?>()).Returns(new List<Transaction>());
        transactionService.ValidateWithCurrentBudgetsAsync(Arg.Any<BudgetCollection?>()).Returns(Task.FromResult(true));
        transactionService.TransactionsListModel.Returns(new TransactionsListModel(logger));

        var userPrompts = new UserPrompts(
            messageBox,
            () => Substitute.For<IUserPromptOpenFile>(),
            () => Substitute.For<IUserPromptSaveFile>(),
            yesNoBox,
            inputBox);

        var newRuleController = new NewRuleController(messenger, logger, userPrompts, transactionRuleService, bucketRepository);
        var editRulesController = new EditRulesController(messenger, userPrompts, newRuleController, transactionRuleService, applicationDatabaseFacade);
        var appliedRulesController = new AppliedRulesController(
            messenger,
            userPrompts,
            editRulesController,
            newRuleController,
            transactionRuleService,
            applicationDatabaseFacade);
        var editingTransactionController = new EditingTransactionController(messenger, bucketRepository);
        var splitTransactionController = new SplitTransactionController(messenger, bucketRepository);
        var loadFileController = new LoadFileController(messenger, userPrompts, accountTypeRepository);
        var fileOperations = new TransactionsControllerFileOperations(
            messenger,
            userPrompts,
            loadFileController,
            applicationDatabaseFacade,
            transactionService);
        var subject = new TopTransactionsListController(
            messenger,
            userPrompts,
            appliedRulesController,
            editingTransactionController,
            splitTransactionController,
            fileOperations,
            transactionService);

        return new TestContext(subject, messenger, transactionService);
    }

    private sealed record TestContext(
        TopTransactionsListController Subject,
        IMessenger Messenger,
        ITransactionManagerService TransactionService);
}
