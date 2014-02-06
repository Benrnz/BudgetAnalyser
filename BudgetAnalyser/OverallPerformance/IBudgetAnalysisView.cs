using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;

namespace BudgetAnalyser.OverallPerformance
{
    public interface IBudgetAnalysisView
    {
        OverallPerformanceBudgetAnalysis Analyse(StatementModel statement, BudgetCollection budgets, GlobalFilterCriteria criteria);

        void ShowDialog(OverallPerformanceBudgetAnalysis analysis);
    }
}