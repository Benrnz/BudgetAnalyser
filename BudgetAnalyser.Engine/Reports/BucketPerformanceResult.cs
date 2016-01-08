using System;
using System.Globalization;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Reports
{
    /// <summary>
    ///     A Data Transfer Object to contain the output of a bucket spending analysis.
    /// </summary>
    public class BucketPerformanceResult
    {
        /// <summary>
        ///     Gets the calculated average spend.
        /// </summary>
        public decimal AverageSpend { get; internal set; }

        /// <summary>
        ///     Gets the calculated balance.
        /// </summary>
        public decimal Balance { get; internal set; }

        /// <summary>
        ///     Gets the bucket.
        /// </summary>
        public BudgetBucket Bucket { get; internal set; }

        /// <summary>
        ///     Gets the budget amount.
        /// </summary>
        public decimal Budget { get; internal set; }

        /// <summary>
        ///     Gets the calculated budget compared to average.
        /// </summary>
        public string BudgetComparedToAverage { get; internal set; }

        /// <summary>
        ///     Gets the calculated budget total.
        /// </summary>
        public decimal BudgetTotal { get; internal set; }

        /// <summary>
        ///     Gets the calculated percentage.
        /// </summary>
        public double Percent
        {
            get
            {
                if (BudgetTotal < 0.01M)
                {
                    return (double) Math.Round(TotalSpent / 0.01M, 2);
                }
                return (double) Math.Round(TotalSpent / BudgetTotal, 2);
            }
        }

        /// <summary>
        ///     Gets the summary.
        /// </summary>
        public string Summary
        {
            get
            {
                var difference = BudgetTotal - TotalSpent;
                if (Percent > 1)
                {
                    return string.Format(
                        CultureInfo.CurrentCulture,
                        "{0:P} ({1:C}) OVER Budget of {2:C}.  Total Spent: {3:C}.  Single Month Budget: {4:C}",
                        Percent - 1,
                        difference,
                        BudgetTotal,
                        TotalSpent,
                        Budget);
                }

                return string.Format(
                    CultureInfo.CurrentCulture,
                    "{0:P} ({1:C}) under Budget of {2:C}.  Total Spent: {3:C}.  Single Month Budget: {4:C}",
                    Percent,
                    difference,
                    BudgetTotal,
                    TotalSpent,
                    Budget);
            }
        }

        /// <summary>
        ///     Gets the calculated total spent.
        /// </summary>
        public decimal TotalSpent { get; internal set; }
    }
}