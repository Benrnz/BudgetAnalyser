using System;
using System.Globalization;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Services
{
    public interface ITransactionManagerService : IServiceFoundation
    {
        StatementModel StatementModel { get; }
        string DetectDuplicateTransactions();
        void FilterTransactions([NotNull] GlobalFilterCriteria criteria);
        void FilterTransactions([NotNull] string searchText);
        void LoadPersistedStateData(object stateData);
        void Merge([NotNull] StatementModel additionalModel);
        object PreparePersistentStateData();
        void RemoveTransaction([NotNull] Transaction transactionToRemove);

        void SplitTransaction(
            [NotNull] Transaction originalTransaction,
            decimal splinterAmount1,
            decimal splinterAmount2,
            [NotNull] BudgetBucket splinterBucket1,
            [NotNull] BudgetBucket splinterBucket2);
    }

    [AutoRegisterWithIoC]
    public class TransactionManagerService : ITransactionManagerService
    {
        public StatementModel StatementModel { get; private set; }

        public string DetectDuplicateTransactions()
        {
            var duplicates = StatementModel.ValidateAgainstDuplicates().ToList();
            return duplicates.Any()
                ? string.Format(CultureInfo.CurrentCulture, "{0} suspected duplicates!", duplicates.Sum(group => group.Count()))
                : null;
        }

        public void FilterTransactions(GlobalFilterCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }

            StatementModel.Filter(criteria);
        }

        public void FilterTransactions(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                throw new ArgumentNullException("searchText");
            }

            StatementModel.FilterByText(searchText);
        }

        public void LoadPersistedStateData(object stateData)
        {
            throw new NotImplementedException();
        }

        public void Merge(StatementModel additionalModel)
        {
            if (additionalModel == null)
            {
                throw new ArgumentNullException("additionalModel");
            }

            StatementModel.Merge(additionalModel);
        }

        public object PreparePersistentStateData()
        {
            throw new NotImplementedException();
            //return new StatementApplicationState()
            //{
            //    StorageKey = StatementModel.StorageKey,
            //    SortByBucket = 

            //}
        }

        public void RemoveTransaction(Transaction transactionToRemove)
        {
            if (transactionToRemove == null)
            {
                throw new ArgumentNullException("transactionToRemove");
            }

            StatementModel.RemoveTransaction(transactionToRemove);
        }

        public void SplitTransaction(Transaction originalTransaction, decimal splinterAmount1, decimal splinterAmount2, BudgetBucket splinterBucket1, BudgetBucket splinterBucket2)
        {
            if (originalTransaction == null)
            {
                throw new ArgumentNullException("originalTransaction");
            }

            if (splinterBucket1 == null)
            {
                throw new ArgumentNullException("splinterBucket1");
            }

            if (splinterBucket2 == null)
            {
                throw new ArgumentNullException("splinterBucket2");
            }

            StatementModel.SplitTransaction(
                originalTransaction,
                splinterAmount1,
                splinterAmount2,
                splinterBucket1,
                splinterBucket2);
        }
    }
}