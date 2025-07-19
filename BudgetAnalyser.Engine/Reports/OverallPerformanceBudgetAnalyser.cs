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

    private DateOnly beginDate;
    private BudgetCollection? budgetCollection;
    private BudgetCycle budgetCycle = BudgetCycle.Monthly;
    private Func<DateOnly, int, DateOnly> calculateNextPeriodDate = (_, _) => throw new NotSupportedException();
    private DateOnly endDate;
    private BudgetModel? latestBudget;
    private GlobalFilterCriteria? rawCriteria;
    private TransactionSetModel? transactionSet;

    /// <summary>
    ///     Analyses the supplied statement using the supplied budget within the criteria given to this method.
    /// </summary>
    /// <param name="budgets">The current budgets collection.</param>
    /// <param name="criteria">The criteria to limit the analysis.</param>
    /// <param name="transactionSetModel">The current statement model.</param>
    /// <exception cref="BudgetException">
    ///     Will be thrown if no budget is supplied or if no budget can be found for the dates
    ///     given in the criteria.
    /// </exception>
    /// <exception cref="ArgumentException">If statement or budget is null.</exception>
    public OverallPerformanceBudgetResult Analyse(TransactionSetModel transactionSetModel,
        BudgetCollection budgets,
        GlobalFilterCriteria criteria)
    {
        this.transactionSet = transactionSetModel;
        this.budgetCollection = budgets;
        this.rawCriteria = criteria;
        AnalysisPreconditions();

        var result = new OverallPerformanceBudgetResult();

        this.latestBudget = EvaluateBudgetsInvolved(result);
        if (result.Error)
        {
            return result;
        }

        CalculateTotalsAndAverage(result);

        result.AnalysesList = new List<BucketPerformanceResult>();
        var list = new List<BucketPerformanceResult>();

        foreach (var bucket in this.bucketRepository.Buckets)
        {
            if (bucket == this.bucketRepository.SurplusBucket)
            {
                list.Add(
                    CalculateBucketStatistics(
                        budget => budget.Surplus,
                        bucket,
                        result.UsesMultipleBudgets,
                        result.DurationInPeriods));
                continue;
            }

            // If the most recent budget does not contain this expense bucket, then skip it.
            if (this.latestBudget.Expenses.Any(e => e.Bucket == bucket))
            {
                list.Add(
                    CalculateBucketStatistics(
                        BuildFunctionToGetBudgetedAmount(bucket),
                        bucket,
                        result.UsesMultipleBudgets,
                        result.DurationInPeriods));
                continue;
            }

            // If the most recent budget does not contain this income bucket, then skip it.
            if (this.latestBudget.Incomes.Any(i => i.Bucket == bucket))
            {
                var incomeAnalysis =
                    CalculateBucketStatistics(
                        BuildFunctionToGetIncomeAmount(bucket),
                        bucket,
                        result.UsesMultipleBudgets,
                        result.DurationInPeriods);

                // Change sign to positive for income
                incomeAnalysis.TotalSpent *= -1;
                incomeAnalysis.AverageSpend *= -1;

                list.Add(incomeAnalysis);
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

        if (this.transactionSet is null)
        {
            throw new ArgumentNullException(nameof(this.transactionSet), "The transaction model supplied is null, analysis cannot proceed with no transactions.");
        }

        if (this.budgetCollection is null)
        {
            throw new ArgumentNullException(nameof(this.budgetCollection));
        }

        if (this.rawCriteria.Cleared)
        {
            this.beginDate = this.transactionSet.AllTransactions.First().Date;
            this.endDate = this.transactionSet.AllTransactions.Last().Date;
        }
        else
        {
            this.beginDate = this.rawCriteria.BeginDate ?? DateOnly.MinValue;
            this.endDate = this.rawCriteria.EndDate ?? DateOnly.MinValue;
        }
    }

    private static Func<BudgetModel, decimal> BuildFunctionToGetBudgetedAmount(BudgetBucket bucket)
    {
        // A little safety net to ensure if the bucket isn't found in the latest budget it simply returns 0.
        return budget =>
        {
            var first = budget.Expenses.FirstOrDefault(e => e.Bucket == bucket);
            if (first is null)
            {
                return 0;
            }

            return first.Amount;
        };
    }

    private static Func<BudgetModel, decimal> BuildFunctionToGetIncomeAmount(BudgetBucket bucket)
    {
        // A little safety net to ensure if the bucket isn't found in the latest budget it simply returns 0.
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

    private BucketPerformanceResult CalculateBucketStatistics(Func<BudgetModel, decimal> getBudgetedAmount, BudgetBucket bucket, bool multipleBudgets, int durationInPeriods)
    {
        var query = this.transactionSet!.Transactions.Where(t => t.BudgetBucket == bucket).ToList();
        var totalSpent = query.Sum(t => t.Amount);
        var averageSpend = totalSpent / durationInPeriods;
        var budgetedTotal = CalculateBudgetedTotalAmount(getBudgetedAmount, multipleBudgets, durationInPeriods);
        var perMonthBudget = budgetedTotal / durationInPeriods;

        return new BucketPerformanceResult
        {
            Bucket = bucket,
            TotalSpent = -totalSpent,
            Balance = budgetedTotal - totalSpent,
            BudgetTotal = budgetedTotal,
            Budget = perMonthBudget,
            AverageSpend = -averageSpend,
            BudgetComparedToAverage = $"Budget per Month: {perMonthBudget:C}, Actual per month: {-averageSpend:C}"
        };
    }

    private decimal CalculateBudgetedTotalAmount(Func<BudgetModel, decimal> getBudgetedAmount, bool multipleBudgets, int durationInPeriods)
    {
        if (!multipleBudgets)
        {
            return getBudgetedAmount(this.latestBudget!) * durationInPeriods;
        }

        decimal budgetedAmount = 0;
        for (var period = 0; period < durationInPeriods; period++)
        {
            var budget = this.budgetCollection!.ForDate(this.calculateNextPeriodDate(this.beginDate, period))
                         ?? this.latestBudget!;
            budgetedAmount += getBudgetedAmount(budget);
        }

        return budgetedAmount;
    }

    private void CalculateTotalsAndAverage(OverallPerformanceBudgetResult result)
    {
        switch (this.budgetCycle)
        {
            case BudgetCycle.Fortnightly:
                result.DurationInPeriods = TransactionsCalculations.CalculateDurationInFortnights(this.rawCriteria, this.transactionSet!.Transactions);
                this.calculateNextPeriodDate = (d, iteration) => d.AddDays(14 * iteration);
                break;
            case BudgetCycle.Monthly:
                result.DurationInPeriods = TransactionsCalculations.CalculateDurationInMonths(this.rawCriteria, this.transactionSet!.Transactions);
                this.calculateNextPeriodDate = (d, iteration) => d.AddMonths(1 * iteration);
                break;
            default:
                throw new NotSupportedException("The Overall Performance Budget Analyser does not support the budget cycle type: " + this.budgetCycle);
        }

        var totalExpensesSpend = this.transactionSet!.Transactions
            .Where(t => t.BudgetBucket is ExpenseBucket)
            .Sum(t => t.Amount);

        var totalSurplusSpend = this.transactionSet!.Transactions
            .Where(t => t.BudgetBucket is SurplusBucket)
            .Sum(t => t.Amount);

        result.AverageSpend = totalExpensesSpend / result.DurationInPeriods; // Expected to be negative
        result.AverageSurplus = totalSurplusSpend / result.DurationInPeriods; // Expected to be negative

        for (var period = 0; period < result.DurationInPeriods; period++)
        {
            var budget = this.budgetCollection!.ForDate(this.calculateNextPeriodDate(this.beginDate, period))
                         ?? this.latestBudget!;
            result.TotalBudgetExpenses += budget.Expenses.Sum(e => e.Amount);
        }

        result.OverallPerformance = totalExpensesSpend + result.TotalBudgetExpenses;
    }

    private BudgetModel EvaluateBudgetsInvolved(OverallPerformanceBudgetResult result)
    {
        var budgetsInvolved = this.budgetCollection!.ForDates(this.beginDate, this.endDate).ToList();
        result.UsesMultipleBudgets = budgetsInvolved.Count() > 1;
        result.ValidationMessage = result.UsesMultipleBudgets ? "Warning! This time period covers multiple budgets." : string.Empty;

        // Use most recent budget as the current, this is only used for showing the latest budget amounts in the report.
        // Calculation of total budgeted amount for the period is done using all budgets involved for the period.
        var currentBudget = budgetsInvolved.Last();

        var hasMultiplePayCycleBudgets = budgetsInvolved.Select(b => b.BudgetCycle).Distinct().Count() > 1;
        if (hasMultiplePayCycleBudgets)
        {
            result.ValidationMessage = "ERROR! This time period covers multiple budgets that use different Pay Cycles (e.g. Fortnightly and Monthly).";
            result.Error = true;
        }

        this.budgetCycle = currentBudget.BudgetCycle;
        result.BudgetCycle = this.budgetCycle.ToString().Replace("ly", "s"); // Hacky but it works. :)

        return currentBudget;
    }
}
