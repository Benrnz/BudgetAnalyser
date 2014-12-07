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
        public decimal AverageSpend { get; internal set; }

        public decimal Balance { get; internal set; }
        public BudgetBucket Bucket { get; internal set; }

        public decimal Budget { get; internal set; }

        public string BudgetComparedToAverage { get; internal set; }
        public decimal BudgetTotal { get; internal set; }

        public double Percent
        {
            get { return (double)Math.Round(TotalSpent / BudgetTotal, 2); }
        }

        public string Summary
        {
            get
            {
                decimal difference = BudgetTotal - TotalSpent;
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

        public decimal TotalSpent { get; internal set; }
    }
}