using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    /// A service class to manage the Burn Down charts.
    /// This service is stateful.
    /// </summary>
    public interface IBurnDownChartsService : IServiceFoundation
    {
        IEnumerable<BudgetBucket> AvailableBucketsForBurnDownCharts();

        BurnDownCharts BuildCharts(
            GlobalFilterCriteria criteria, 
            StatementModel statementModel, 
            BudgetModel budgetModel, 
            LedgerBook ledgerBookModel);

        BurnDownChartAnalyserResult CreateNewCustomAggregateChart(
            StatementModel statementModel, 
            BudgetModel budgetModel, 
            IEnumerable<BudgetBucket> buckets,
            LedgerBook ledgerBookModel,
            DateTime beginDate, 
            string chartTitle);

        void LoadPersistedStateData(object persistedStateData);

        object PreparePersistentStateData();
        
        void RemoveCustomChart(string chartName);
    }
}