using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.ReportsCatalog.OverallPerformance;
using BudgetAnalyser.Transactions;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.ReportsCatalog;

[AutoRegisterWithIoC(SingleInstance = true)]
public class TopReportsCatalogController : ControllerBase, IShowableController
{
    private readonly IUserInputBox inputBox;
    private readonly NewWindowViewLoader newWindowViewLoader;
    private BudgetCollection? budgets;
    private TransactionsListModel? currentTransactionsListModel;

    public TopReportsCatalogController(IMessenger messenger, OverallPerformanceController overallPerformanceController, NewWindowViewLoader newWindowViewLoader, UserPrompts userPrompts) :
        base(messenger)
    {
        this.newWindowViewLoader = newWindowViewLoader ?? throw new ArgumentNullException(nameof(newWindowViewLoader));
        OverallPerformanceController = overallPerformanceController ?? throw new ArgumentNullException(nameof(overallPerformanceController));
        this.inputBox = userPrompts.InputBox ?? throw new ArgumentNullException(nameof(userPrompts));

        Messenger.Register<TopReportsCatalogController, TransactionsListModelReadyMessage>(this, OnTransactionsReadyMessageReceived);
        Messenger.Register<TopReportsCatalogController, BudgetReadyMessage>(this, OnBudgetReadyMessageReceived);
    }

    /// <summary>
    ///     Used by the styles as the styles are shared with widgets. The tile can be dynamically enabled or disabled.
    /// </summary>
    [UsedImplicitly]
    public bool Enabled => true;

    public OverallPerformanceController OverallPerformanceController { get; }

    public bool OverallPerformanceReportIsAvailable =>
        this.currentTransactionsListModel is not null
        && this.currentTransactionsListModel.Transactions.Any()
        && this.budgets?.CurrentActiveBudget is not null;

    public bool Shown
    {
        get;
        set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public void ShowOverallPerformanceReport()
    {
        if (this.currentTransactionsListModel is null || this.budgets is null)
        {
            return;
        }

        var (date1, date2) = RequestCurrentFilter();
        OverallPerformanceController.Load(this.currentTransactionsListModel, this.budgets, date1!.Value, date2!.Value);

        this.newWindowViewLoader.MinHeight = this.newWindowViewLoader.Height = 650;
        this.newWindowViewLoader.MinWidth = this.newWindowViewLoader.Width = 740;
        this.newWindowViewLoader.Show(OverallPerformanceController);
    }

    private void OnBudgetReadyMessageReceived(TopReportsCatalogController recipient, BudgetReadyMessage message)
    {
        this.budgets = message.Budgets;
        OnPropertyChanged(nameof(OverallPerformanceReportIsAvailable));
    }

    private void OnTransactionsReadyMessageReceived(TopReportsCatalogController recipient, TransactionsListModelReadyMessage message)
    {
        this.currentTransactionsListModel = message.Model;
        OnPropertyChanged(nameof(OverallPerformanceReportIsAvailable));
    }

    private (DateOnly?, DateOnly?) RequestCurrentFilter()
    {
        var date1 = DateOnly.MinValue;
        var date2 = DateOnly.MinValue;

        var defaultValue = DateTime.Today.FirstDateInMonth().AddMonths(-12).ToString("yyyy-MM-dd");
        while (date1 == DateOnly.MinValue)
        {
            var input = this.inputBox.Show("Enter the start date for the report in the format YYYY-MM-DD", "Report Start Date", defaultValue);
            if (DateOnly.TryParse(input, out var date))
            {
                date1 = date;
            }
        }

        defaultValue = DateTime.Today.FirstDateInMonth().ToString("yyyy-MM-dd");
        while (date2 == DateOnly.MinValue)
        {
            var input = this.inputBox.Show("Enter the end date for the report in the format YYYY-MM-DD", "Report End Date", defaultValue);
            if (DateOnly.TryParse(input, out var date))
            {
                date2 = date;
            }
        }

        return (date1, date2);
    }
}
