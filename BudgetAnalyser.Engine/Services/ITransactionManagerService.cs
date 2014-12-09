using System;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Services
{
    public interface ITransactionManagerService : IServiceFoundation
    {
        StatementModel StatementModel { get; }

        object PreparePersistentStateData();
        void LoadPersistedStateData(object stateData);
    }

    [AutoRegisterWithIoC]
    public class TransactionManagerService : ITransactionManagerService
    {
        public StatementModel StatementModel { get; private set; }

        public object PreparePersistentStateData()
        {
            throw new NotImplementedException();
            //return new StatementApplicationState()
            //{
            //    StorageKey = StatementModel.StorageKey,
            //    SortByBucket = 

            //}
        }

        public void LoadPersistedStateData(object stateData)
        {
            throw new System.NotImplementedException();
        }
    }
}
