using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    ///     A service class to manage the Burn Down charts.
    ///     This service is stateful.
    /// </summary>
    public interface IBurnDownChartsService : IServiceFoundation
    {
        /// <summary>
        ///     Returns a list of available buckets for custom burn down charts.
        /// </summary>
        IEnumerable<BudgetBucket> AvailableBucketsForBurnDownCharts();

        /// <summary>
        ///     Builds all the charts.
        /// </summary>
        /// <param name="statementModel">The current statement model.</param>
        /// <param name="budgetModel">The current budget model.</param>
        /// <param name="ledgerBookModel">The current ledger book model.</param>
        /// <param name="criteria">The global filter criteria.</param>
        /// <returns>An object that contains all compiled information for the charts including the chart data itself.</returns>
        BurnDownCharts BuildAllCharts(StatementModel statementModel, BudgetModel budgetModel, LedgerBook ledgerBookModel,
                                      [NotNull] GlobalFilterCriteria criteria);

        /// <summary>
        ///     Creates a new custom aggregate chart that combines multiple bucket spending information into one burn down chart.
        /// </summary>
        /// <param name="statementModel">The current statement model.</param>
        /// <param name="budgetModel">The current budget model.</param>
        /// <param name="buckets">The current buckets.</param>
        /// <param name="ledgerBookModel">The current ledger book model.</param>
        /// <param name="beginDate">The begin date.</param>
        /// <param name="chartTitle">The chart title.</param>
        /// <returns>An object that contains data for a single chart, the new one just created.</returns>
        BurnDownChartAnalyserResult CreateNewCustomAggregateChart(
            StatementModel statementModel,
            BudgetModel budgetModel,
            IEnumerable<BudgetBucket> buckets,
            LedgerBook ledgerBookModel,
            DateTime beginDate,
            string chartTitle);

        /// <summary>
        ///     Loads the persisted state data.
        /// </summary>
        /// <param name="persistedStateData">The persisted state data.</param>
        void LoadPersistedStateData([NotNull] CustomBurnDownChartApplicationState persistedStateData);

        /// <summary>
        ///     Prepares the persistent state data.
        /// </summary>
        /// <returns></returns>
        CustomBurnDownChartApplicationState PreparePersistentStateData();

        /// <summary>
        ///     Removes a custom chart from the charts.
        /// </summary>
        /// <param name="chartName">Name of the chart to remove.</param>
        void RemoveCustomChart(string chartName);
    }
}