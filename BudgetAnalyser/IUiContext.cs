using System.Collections.Generic;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.Matching;
using BudgetAnalyser.ReportsCatalog;
using BudgetAnalyser.Statement;
using GalaSoft.MvvmLight.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser
{
    /// <summary>
    ///     Controllers required by the <see cref="ShellController" /> and most other <see cref="ControllerBase" /> controllers
    ///     grouped together for convenience.
    ///     This follows an Ambient Context pattern. Not using Thread Local Storage for ease of testing.
    ///     The idea to prevent ambient UI context objects that are required by all UI Controllers and ViewModels from
    ///     appearing in every constructor.
    /// </summary>
    public interface IUiContext
    {
        AppliedRulesController AppliedRulesController { get; }
        BudgetController BudgetController { get; }
        ChooseBudgetBucketController ChooseBudgetBucketController { get; }
        IEnumerable<ControllerBase> Controllers { get; }
        CreateNewFixedBudgetController CreateNewFixedBudgetController { get; }
        CreateNewSurprisePaymentMonitorController CreateNewSurprisePaymentMonitorController { get; }
        DashboardController DashboardController { get; }
        EditingTransactionController EditingTransactionController { get; }
        EncryptFileController EncryptFileController { get; }
        GlobalFilterController GlobalFilterController { get; }
        LedgerBookController LedgerBookController { get; }
        ILogger Logger { get; }
        MainMenuController MainMenuController { get; }
        IMessenger Messenger { get; }
        NewRuleController NewRuleController { get; }
        ReportsCatalogController ReportsCatalogController { get; }
        RulesController RulesController { get; }
        IEnumerable<IShowableController> ShowableControllers { get; }
        SplitTransactionController SplitTransactionController { get; }
        StatementController StatementController { get; }
        UserPrompts UserPrompts { get; }
    }
}