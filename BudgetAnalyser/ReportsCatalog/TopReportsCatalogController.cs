using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.ReportsCatalog.OverallPerformance;
using BudgetAnalyser.Transactions;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.ReportsCatalog;

[AutoRegisterWithIoC(SingleInstance = true)]
public class TopReportsCatalogController : ControllerBase, IShowableController
{
    private readonly NewWindowViewLoader newWindowViewLoader;
    private BudgetCollection? budgets;
    private TransactionsListModel? currentTransactionsListModel;

    public TopReportsCatalogController(IUiContext uiContext, NewWindowViewLoader newWindowViewLoader) : base(uiContext.Messenger)
    {
        if (uiContext is null)
        {
            throw new ArgumentNullException(nameof(uiContext));
        }

        this.newWindowViewLoader = newWindowViewLoader ?? throw new ArgumentNullException(nameof(newWindowViewLoader));
        OverallPerformanceController = uiContext.Controller<OverallPerformanceController>();

        Messenger.Register<TopReportsCatalogController, TransactionsListModelReadyMessage>(this, static (r, m) => r.OnTransactionsReadyMessageReceived(m));
        Messenger.Register<TopReportsCatalogController, BudgetReadyMessage>(this, static (r, m) => r.OnBudgetReadyMessageReceived(m));
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

        var filter = RequestCurrentFilter() ?? new GlobalFilterCriteria();
        OverallPerformanceController.Load(this.currentTransactionsListModel, this.budgets, filter);

        this.newWindowViewLoader.MinHeight = this.newWindowViewLoader.Height = 650;
        this.newWindowViewLoader.MinWidth = this.newWindowViewLoader.Width = 740;
        this.newWindowViewLoader.Show(OverallPerformanceController);
    }

    private void OnBudgetReadyMessageReceived(BudgetReadyMessage message)
    {
        this.budgets = message.Budgets;
        OnPropertyChanged(nameof(OverallPerformanceReportIsAvailable));
    }

    private void OnTransactionsReadyMessageReceived(TransactionsListModelReadyMessage message)
    {
        this.currentTransactionsListModel = message.Model;
        OnPropertyChanged(nameof(OverallPerformanceReportIsAvailable));
    }

    private GlobalFilterCriteria? RequestCurrentFilter()
    {
        // TODO it would be better if reporting operated on its own independent date range to the global filter.
        var currentFilterMessage = new RequestFilterMessage(this);
        Messenger.Send(currentFilterMessage);
        return currentFilterMessage.Criteria;
    }
}
