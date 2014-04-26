using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Statement
{
    public class TransactionGroupedByBucket : INotifyPropertyChanged
    {
        public TransactionGroupedByBucket(IEnumerable<Transaction> transactions, BudgetBucket groupByThisBucket)
        {
            Bucket = groupByThisBucket;
            Transactions = new ObservableCollection<Transaction>(transactions.Where(t => t.BudgetBucket == groupByThisBucket).OrderBy(t => t.Date));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public BudgetBucket Bucket { get; private set; }

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
                    return query.Average(t => t.Amount);
                }

                return 0;
            }
        }

        public bool HasTransactions
        {
            get { return Transactions != null && Transactions.Any(); }
        }

        public DateTime MaxTransactionDate
        {
            get { return Transactions.Max(t => t.Date); }
        }

        public DateTime MinTransactionDate
        {
            get { return Transactions.Min(t => t.Date); }
        }

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

        public decimal TotalDifference
        {
            get { return TotalCredits + TotalDebits; }
        }

        public ObservableCollection<Transaction> Transactions { get; private set; }

        public void TriggerRefreshTotalsRow()
        {
            OnPropertyChanged("TotalCredits");
            OnPropertyChanged("TotalDebits");
            OnPropertyChanged("TotalDifference");
            OnPropertyChanged("AverageDebit");
            OnPropertyChanged("TotalCount");
            OnPropertyChanged("HasTransactions");
            OnPropertyChanged("MinTransactionDate");
            OnPropertyChanged("MaxTransactionDate");
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}