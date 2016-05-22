using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using BudgetAnalyser.Engine.Budget;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    ///     A set of grouped <see cref="Transaction" />s with aggregated summary information.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class TransactionGroupedByBucket : INotifyPropertyChanged
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionGroupedByBucket" /> class.
        /// </summary>
        /// <param name="transactions">The transactions.</param>
        /// <param name="groupByThisBucket">The group by this bucket.</param>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public TransactionGroupedByBucket([NotNull] IEnumerable<Transaction> transactions,
                                          [NotNull] BudgetBucket groupByThisBucket)
        {
            if (transactions == null)
            {
                throw new ArgumentNullException(nameof(transactions));
            }

            if (groupByThisBucket == null)
            {
                throw new ArgumentNullException(nameof(groupByThisBucket));
            }

            Bucket = groupByThisBucket;
            Transactions =
                new ObservableCollection<Transaction>(
                    transactions.Where(t => t.BudgetBucket == groupByThisBucket).OrderBy(t => t.Date));
        }

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Gets the average debit amount.
        /// </summary>
        public decimal AverageDebit
        {
            get
            {
                if (Transactions == null)
                {
                    return 0;
                }

                IEnumerable<Transaction> query = Transactions.Where(t => t.Amount < 0).ToList();
                if (query.Any())
                {
                    return query.SafeAverage(t => t.Amount);
                }

                return 0;
            }
        }

        /// <summary>
        ///     Gets the bucket the transactions are grouped by.
        /// </summary>
        public BudgetBucket Bucket { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether this group has any transactions.
        /// </summary>
        public bool HasTransactions => Transactions != null && Transactions.Any();

        /// <summary>
        ///     Gets the latest transaction date.
        /// </summary>
        public DateTime MaxTransactionDate => Transactions.Max(t => t.Date);

        /// <summary>
        ///     Gets the earliest transaction date.
        /// </summary>
        public DateTime MinTransactionDate => Transactions.Min(t => t.Date);

        /// <summary>
        ///     Gets the total count of transations in the group.
        /// </summary>
        public decimal TotalCount
        {
            get
            {
                if (Transactions == null)
                {
                    return 0;
                }

                return Transactions.Count();
            }
        }

        /// <summary>
        ///     Gets the total credits of all transactions in the group.
        /// </summary>
        public decimal TotalCredits
        {
            get
            {
                if (Transactions == null)
                {
                    return 0;
                }

                return Transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
            }
        }

        /// <summary>
        ///     Gets the total debits of all transactions in the group.
        /// </summary>
        public decimal TotalDebits
        {
            get
            {
                if (Transactions == null)
                {
                    return 0;
                }

                return Transactions.Where(t => t.Amount < 0).Sum(t => t.Amount);
            }
        }

        /// <summary>
        ///     Gets the total difference between all credits and debits.
        /// </summary>
        public decimal TotalDifference => TotalCredits + TotalDebits;

        /// <summary>
        ///     Gets the grouped transactions.
        /// </summary>
        public ObservableCollection<Transaction> Transactions { get; }

        /// <summary>
        ///     Triggers a refresh of the totals row.
        /// </summary>
        public void TriggerRefreshTotalsRow()
        {
            OnPropertyChanged(nameof(TotalCredits));
            OnPropertyChanged(nameof(TotalDebits));
            OnPropertyChanged(nameof(TotalDifference));
            OnPropertyChanged(nameof(AverageDebit));
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(HasTransactions));
            OnPropertyChanged(nameof(MinTransactionDate));
            OnPropertyChanged(nameof(MaxTransactionDate));
        }

        /// <summary>
        ///     Called when a property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}