using System.Globalization;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Reports;

/// <summary>
///     An analyser class to build report data for the overall performance report.
/// </summary>
[AutoRegisterWithIoC]
internal class OverallPerformanceBudgetAnalyser(IBudgetBucketRepository bucketRepository)
{
    private readonly IBudgetBucketRepository bucketRepository = bucketRepository ?? throw new ArgumentNullException(nameof(bucketRepository));

    private DateTime beginDate;
    private BudgetCollection? budgetCollection;
    private BudgetCycle budgetCycle = BudgetCycle.Monthly;
    private DateTime endDate;
    private GlobalFilterCriteria? rawCriteria;
    private StatementModel? statement;

    /// <summary>
    ///     Analyses the supplied statement using the supplied budget within the criteria given to this method.
    /// </summary>
    /// <param name="budgets">The current budgets collection.</param>
    /// <param name="criteria">The criteria to limit the analysis.</param>
    /// <param name="statementModel">The current statement model.</param>
    /// <exception cref="BudgetException">
    ///     Will be thrown if no budget is supplied or if no budget can be found for the dates
    ///     given in the criteria.
    /// </exception>
    /// <exception cref="ArgumentException">If statement or budget is null.</exception>
    public OverallPerformanceBudgetResult Analyse(StatementModel statementModel,
        BudgetCollection budgets,
        GlobalFilterCriteria criteria)
    {
        this.statement = statementModel;
        this.budgetCollection = budgets;
        this.rawCriteria = criteria;
        AnalysisPreconditions();

        var result = new OverallPerformanceBudgetResult();

        var currentBudget = EvaluateBudgetsInvolved(result);
        if (result.Error)
        {
            return result;
        }

        CalculateTotalsAndAverage(result);

        result.AnalysesList = new List<BucketPerformanceResult>();
        var list = new List<BucketPerformanceResult>();

        foreach (var bucket in this.bucketRepository.Buckets)
        {
            var bucketCopy = bucket;
            var query = statementModel.Transactions.Where(t => t.BudgetBucket == bucketCopy).ToList();
            var totalSpent = query.Sum(t => t.Amount);
            var averageSpend = totalSpent / result.DurationInPeriods;

            if (bucket == this.bucketRepository.SurplusBucket)
            {
                var budgetedTotal = CalculateBudgetedTotalAmount(this.beginDate, b => b.Surplus, budgets, result);
                var perMonthBudget = budgetedTotal / result.DurationInPeriods;
                // Calc an average in case multiple budgets are used and the budgeted amounts are different.
                var surplusAnalysis = new BucketPerformanceResult
                {
                    Bucket = bucket,
                    TotalSpent = -totalSpent,
                    Balance = budgetedTotal - totalSpent,
                    BudgetTotal = budgetedTotal,
                    Budget = perMonthBudget,
                    AverageSpend = -averageSpend,
                    BudgetComparedToAverage =
                        string.Format(CultureInfo.CurrentCulture, "Budget per Month: {0:C}, Actual per Month: {1:C}",
                            perMonthBudget, -averageSpend)
                };
                list.Add(surplusAnalysis);
                continue;
            }

            // If the most recent budget does not contain this expense bucket, then skip it.
            if (currentBudget.Expenses.Any(e => e.Bucket == bucket))
            {
                var totalBudget = CalculateBudgetedTotalAmount(this.beginDate, BuildExpenseFinder(bucket), budgets, result);
                var perMonthBudget = totalBudget / result.DurationInPeriods;
                var analysis = new BucketPerformanceResult
                {
                    Bucket = bucket,
                    TotalSpent = -totalSpent,
                    Balance = totalBudget - totalSpent,
                    BudgetTotal = totalBudget,
                    Budget = perMonthBudget,
                    AverageSpend = -averageSpend,
                    BudgetComparedToAverage =
                        string.Format(CultureInfo.CurrentCulture, "Budget per Month: {0:C}, Actual per Month: {1:C}",
                            perMonthBudget, -averageSpend)
                };
                list.Add(analysis);
                continue;
            }

            // If the most recent budget does not contain this income bucket, then skip it.
            if (currentBudget.Incomes.Any(i => i.Bucket == bucket))
            {
                var totalBudget = CalculateBudgetedTotalAmount(this.beginDate, BuildIncomeFinder(bucket), budgets, result);
                var perMonthBudget = totalBudget / result.DurationInPeriods;
                var analysis = new BucketPerformanceResult
                {
                    Bucket = bucket,
                    TotalSpent = totalSpent,
                    Balance = totalBudget - totalSpent,
                    BudgetTotal = totalBudget,
                    Budget = perMonthBudget,
                    AverageSpend = averageSpend,
                    BudgetComparedToAverage =
                        string.Format(CultureInfo.CurrentCulture, "Budget per Month: {0:C}, Actual per month: {1:C}",
                            perMonthBudget, -averageSpend)
                };
                list.Add(analysis);
            }
        } // Foreach Bucket

        result.AnalysesList = list.OrderByDescending(a => a.Percent).ToList();
        return result;
    }

    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local - purpose of this function is to do pre-condition checks.
    private void AnalysisPreconditions()
    {
        if (this.rawCriteria is null)
        {
            throw new ArgumentNullException(nameof(this.rawCriteria));
        }

        if (!this.rawCriteria.Cleared && (this.rawCriteria.BeginDate is null || this.rawCriteria.EndDate is null))
        {
            throw new ArgumentException("The given criteria does not contain any filtering dates.");
        }

        if (this.statement is null)
        {
            throw new ArgumentNullException(nameof(this.statement), "The statement supplied is null, analysis cannot proceed with no statement.");
        }

        if (this.budgetCollection is null)
        {
            throw new ArgumentNullException(nameof(this.budgetCollection));
        }

        if (this.rawCriteria.Cleared)
        {
            this.beginDate = this.statement.AllTransactions.First().Date;
            this.endDate = this.statement.AllTransactions.Last().Date;
        }
        else
        {
            this.beginDate = this.rawCriteria.BeginDate ?? DateTime.MinValue;
            this.endDate = this.rawCriteria.EndDate ?? DateTime.MinValue;
        }
    }

    private static Func<BudgetModel, decimal> BuildExpenseFinder(BudgetBucket bucket)
    {
        return b =>
        {
            var first = b.Expenses.FirstOrDefault(e => e.Bucket == bucket);
            if (first is null)
            {
                return 0;
            }

            return first.Amount;
        };
    }

    private static Func<BudgetModel, decimal> BuildIncomeFinder(BudgetBucket bucket)
    {
        return b =>
        {
            var first = b.Incomes.FirstOrDefault(e => e.Bucket == bucket);
            if (first is null)
            {
                return 0;
            }

            return first.Amount;
        };
    }

    private static decimal CalculateBudgetedTotalAmount(DateTime beginDate,
        Func<BudgetModel, decimal> whichBudgetBucket,
        BudgetCollection budgets,
        OverallPerformanceBudgetResult result)
    {
        if (!result.UsesMultipleBudgets)
        {
            return whichBudgetBucket(budgets.ForDate(beginDate)) * result.DurationInPeriods;
        }

        decimal budgetedAmount = 0;
        for (var month = 0; month < result.DurationInPeriods; month++)
        {
            var budget = budgets.ForDate(beginDate.AddMonths(month));
            budgetedAmount += whichBudgetBucket(budget);
        }

        return budgetedAmount;
    }

    private void CalculateTotalsAndAverage(OverallPerformanceBudgetResult result)
    {
        Func<DateTime, int, DateTime> calculateNextPeriodDate;
        switch (this.budgetCycle)
        {
            case BudgetCycle.Fortnightly:
                result.DurationInPeriods = StatementCalculations.CalculateDurationInFortnights(this.rawCriteria, this.statement!.Transactions);
                calculateNextPeriodDate = (d, iteration) => d.AddDays(14 * iteration);
                break;
            case BudgetCycle.Monthly:
                result.DurationInPeriods = StatementCalculations.CalculateDurationInMonths(this.rawCriteria, this.statement!.Transactions);
                calculateNextPeriodDate = (d, iteration) => d.AddMonths(1 * iteration);
                break;
            default:
                throw new NotSupportedException("The Overall Performance Budget Analyser does not support the budget cycle type: " + this.budgetCycle);
        }

        var totalExpensesSpend = this.statement!.Transactions
            .Where(t => t.BudgetBucket is ExpenseBucket)
            .Sum(t => t.Amount);

        var totalSurplusSpend = this.statement!.Transactions
            .Where(t => t.BudgetBucket is SurplusBucket)
            .Sum(t => t.Amount);

        result.AverageSpend = totalExpensesSpend / result.DurationInPeriods; // Expected to be negative
        result.AverageSurplus = totalSurplusSpend / result.DurationInPeriods; // Expected to be negative

        for (var period = 0; period < result.DurationInPeriods; period++)
        {
            var budget = this.budgetCollection!.ForDate(calculateNextPeriodDate(this.beginDate, period));
            result.TotalBudgetExpenses += budget.Expenses.Sum(e => e.Amount);
        }

        result.OverallPerformance = result.AverageSpend + result.TotalBudgetExpenses;
    }

    private BudgetModel EvaluateBudgetsInvolved(OverallPerformanceBudgetResult result)
    {
        var budgetsInvolved = this.budgetCollection!.ForDates(this.beginDate, this.endDate).ToList();
        result.UsesMultipleBudgets = budgetsInvolved.Count() > 1;
        result.ValidationMessage = result.UsesMultipleBudgets ? "Warning! This time period covers multiple budgets." : string.Empty;

        // Use most recent budget as the current, I realise this isn't optimal, but better to estimate future budgets from most current budget.
        var currentBudget = budgetsInvolved.Last();

        var hasMultiplePayCycleBudgets = budgetsInvolved.Select(b => b.BudgetCycle).Distinct().Count() > 1;
        if (hasMultiplePayCycleBudgets)
        {
            result.ValidationMessage = "ERROR! This time period covers multiple budgets that use different Pay Cycles (e.g. Fortnightly and Monthly).";
            result.Error = true;
        }

        this.budgetCycle = currentBudget.BudgetCycle;
        result.BudgetCycle = this.budgetCycle.ToString().Replace("ly", "s");

        return currentBudget;
    }
}
