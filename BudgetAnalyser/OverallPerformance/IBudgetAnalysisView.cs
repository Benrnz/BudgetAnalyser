using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.OverallPerformance
{
    public interface IBudgetAnalysisView
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification="Custom collection")]
        OverallPerformanceBudgetAnalysis Analyse(StatementModel statement, BudgetCollection budgets, GlobalFilterCriteria criteria);

        void ShowDialog(OverallPerformanceBudgetAnalysis analysis);
    }
}