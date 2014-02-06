using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Reports
{
    public class OverallPerformanceBudgetAnalysis
    {
        private readonly IBudgetBucketRepository bucketRepository;
        private readonly BudgetCollection budgets;
        private readonly StatementModel statement;

        public OverallPerformanceBudgetAnalysis([NotNull] StatementModel statement, [NotNull] BudgetCollection budgets, [NotNull] IBudgetBucketRepository bucketRepository)
        {
            if (statement == null)
            {
                throw new ArgumentNullException("statement");
            }

            if (budgets == null)
            {
                throw new ArgumentNullException("budgets");
            }

            if (bucketRepository == null)
            {
                throw new ArgumentNullException("bucketRepository");
            }

            this.statement = statement;
            this.budgets = budgets;
            this.bucketRepository = bucketRepository;
        }

        public List<BucketAnalysis> Analyses { get; private set; }

        /// <summary>
        ///     Gets the average spend per month based on statement transaction data over a period of time.
        ///     Expected to be negative.
        /// </summary>
        public decimal AverageSpend { get; private set; }

        /// <summary>
        ///     Gets the average surplus spending per month based on statement transaction data over a period of time.
        /// </summary>
        public decimal AverageSurplus { get; private set; }

        public int DurationInMonths { get; private set; }

        public decimal OverallPerformance { get; private set; }
        public decimal TotalBudgetExpenses { get; private set; }

        public bool UsesMultipleBudgets { get; private set; }

        /// <summary>
        ///     Analyses the supplied statement using the supplied budget within the criteria given to this method.
        /// </summary>
        /// <param name="criteria">The criteria to limit the analysis.</param>
        /// <exception cref="BudgetException">
        ///     Will be thrown if no budget is supplied or if no budget can be found for the dates
        ///     given in the criteria.
        /// </exception>
        /// <exception cref="ArgumentException">If statement or budget is null.</exception>
        public void Analyse([NotNull] GlobalFilterCriteria criteria)
        {
            DateTime endDate, beginDate;
            AnalysisPreconditions(criteria, out beginDate, out endDate);

            List<BudgetModel> budgetsInvolved = this.budgets.ForDates(beginDate, endDate).ToList();
            UsesMultipleBudgets = budgetsInvolved.Count() > 1;
            BudgetModel currentBudget = budgetsInvolved.First();

            DurationInMonths = StatementModel.CalculateDuration(criteria, this.statement.Transactions);

            CalculateTotalsAndAverage(currentBudget);

            Analyses = new List<BucketAnalysis>();
            var list = new List<BucketAnalysis>();
            foreach (BudgetBucket bucket in this.bucketRepository.Buckets)
            {
                BudgetBucket bucketCopy = bucket;
                List<Transaction> query = this.statement.Transactions.Where(t => t.BudgetBucket == bucketCopy).ToList();
                decimal totalSpent = query.Sum(t => t.Amount);
                decimal averageSpend = totalSpent/DurationInMonths;

                if (bucket == this.bucketRepository.SurplusBucket)
                {
                    var surplusAnalysis = new BucketAnalysis
                    {
                        Bucket = bucket,
                        TotalSpent = -totalSpent,
                        Balance = currentBudget.Surplus*DurationInMonths - totalSpent,
                        BudgetTotal = currentBudget.Surplus*DurationInMonths,
                        Budget = currentBudget.Surplus,
                        AverageSpend = -averageSpend,
                        BudgetComparedToAverage =
                            string.Format("Budget per Month: {0:C}, Actual per Month: {1:C}", currentBudget.Surplus,
                                -averageSpend)
                    };
                    list.Add(surplusAnalysis);
                    continue;
                }

                Expense expense = currentBudget.Expenses.FirstOrDefault(e => e.Bucket == bucket);
                if (expense != null)
                {
                    decimal totalBudget = expense.Amount*DurationInMonths;
                    var analysis = new BucketAnalysis
                    {
                        Bucket = bucket,
                        TotalSpent = -totalSpent,
                        Balance = totalBudget - totalSpent,
                        BudgetTotal = totalBudget,
                        Budget = expense.Amount,
                        AverageSpend = -averageSpend,
                        BudgetComparedToAverage =
                            string.Format("Budget per Month: {0:C}, Actual per Month: {1:C}", expense.Amount,
                                -averageSpend)
                    };
                    list.Add(analysis);
                    continue;
                }

                Income income = currentBudget.Incomes.FirstOrDefault(i => i.Bucket == bucket);
                if (income != null)
                {
                    decimal totalBudget = income.Amount*DurationInMonths;
                    var analysis = new BucketAnalysis
                    {
                        Bucket = bucket,
                        TotalSpent = totalSpent,
                        Balance = totalBudget - totalSpent,
                        BudgetTotal = totalBudget,
                        Budget = income.Amount,
                        AverageSpend = averageSpend,
                        BudgetComparedToAverage =
                            string.Format("Budget per Month: {0:C}, Actual per month: {1:C}", income.Amount,
                                -averageSpend)
                    };
                    list.Add(analysis);
                }
            }

            Analyses = list.OrderByDescending(a => a.Percent).ToList();
        }

        private void AnalysisPreconditions(GlobalFilterCriteria criteria, out DateTime beginDate, out DateTime endDate)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }

            if (!criteria.Cleared && (criteria.BeginDate == null || criteria.EndDate == null))
            {
                throw new ArgumentException("The given criteria does not contain any filtering dates.");
            }

            if (this.statement == null)
            {
                throw new ArgumentException("The statement supplied is null, analysis cannot proceed with no statement.");
            }

            if (this.budgets == null)
            {
                throw new ArgumentException("budgets");
            }

            if (criteria.Cleared)
            {
                beginDate = this.statement.AllTransactions.First().Date;
                endDate = this.statement.AllTransactions.Last().Date;
            }
            else
            {
                beginDate = criteria.BeginDate.Value;
                endDate = criteria.EndDate.Value;
            }
        }

        private void CalculateTotalsAndAverage(BudgetModel currentBudget)
        {
            // First total the expenses without the saved up for expenses.
            decimal totalExpensesSpend = this.statement.Transactions
                .Where(t => t.BudgetBucket is ExpenseBudgetBucket)
                .Sum(t => t.Amount);

            decimal totalSurplusSpend = this.statement.Transactions
                .Where(t => t.BudgetBucket is SurplusBucket)
                .Sum(t => t.Amount);

            AverageSpend = totalExpensesSpend/DurationInMonths; // Expected to be negative
            AverageSurplus = totalSurplusSpend/DurationInMonths; // Expected to be negative
            TotalBudgetExpenses = currentBudget.Expenses.Sum(e => e.Amount);
            OverallPerformance = AverageSpend + TotalBudgetExpenses;
        }
    }
}