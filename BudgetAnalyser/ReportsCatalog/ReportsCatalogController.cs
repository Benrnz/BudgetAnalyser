using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.ReportsCatalog.OverallPerformance;
using BudgetAnalyser.Statement;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.ReportsCatalog;

[AutoRegisterWithIoC(SingleInstance = true)]
public class ReportsCatalogController : ControllerBase, IShowableController
{
    private readonly NewWindowViewLoader newWindowViewLoader;
    private BudgetCollection? budgets;
    private StatementModel? currentStatementModel;
    private bool doNotUseShown;

    public ReportsCatalogController(IUiContext uiContext, NewWindowViewLoader newWindowViewLoader) : base(uiContext.Messenger)
    {
        if (uiContext is null)
        {
            throw new ArgumentNullException(nameof(uiContext));
        }

        this.newWindowViewLoader = newWindowViewLoader ?? throw new ArgumentNullException(nameof(newWindowViewLoader));
        OverallPerformanceController = uiContext.Controller<OverallPerformanceController>();

        Messenger.Register<ReportsCatalogController, StatementReadyMessage>(this, static (r, m) => r.OnStatementReadyMessageReceived(m));
        Messenger.Register<ReportsCatalogController, BudgetReadyMessage>(this, static (r, m) => r.OnBudgetReadyMessageReceived(m));
    }

    /// <summary>
    ///     Used by the styles as the styles are shared with widgets. The tile can be dynamically enabled or disabled.
    /// </summary>
    [UsedImplicitly]
    public bool Enabled => true;

    public OverallPerformanceController OverallPerformanceController { get; }

    public bool OverallPerformanceReportIsAvailable =>
        this.currentStatementModel is not null
        && this.currentStatementModel.Transactions.Any()
        && this.budgets?.CurrentActiveBudget is not null;

    public bool Shown
    {
        get => this.doNotUseShown;
        set
        {
            if (value == this.doNotUseShown)
            {
                return;
            }

            this.doNotUseShown = value;
            OnPropertyChanged();
        }
    }

    public void ShowOverallPerformanceReport()
    {
        if (this.currentStatementModel is null || this.budgets is null)
        {
            return;
        }

        var filter = RequestCurrentFilter() ?? new GlobalFilterCriteria();
        OverallPerformanceController.Load(this.currentStatementModel, this.budgets, filter);

        this.newWindowViewLoader.MinHeight = this.newWindowViewLoader.Height = 650;
        this.newWindowViewLoader.MinWidth = this.newWindowViewLoader.Width = 740;
        this.newWindowViewLoader.Show(OverallPerformanceController);
    }

    private void OnBudgetReadyMessageReceived(BudgetReadyMessage message)
    {
        this.budgets = message.Budgets;
        OnPropertyChanged(nameof(OverallPerformanceReportIsAvailable));
    }

    private void OnStatementReadyMessageReceived(StatementReadyMessage message)
    {
        this.currentStatementModel = message.StatementModel;
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
