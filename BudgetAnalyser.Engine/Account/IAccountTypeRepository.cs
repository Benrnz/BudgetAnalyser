using System;
using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Account
{
    public interface IAccountTypeRepository
    {
        AccountType Add(string key, AccountType instance);

        AccountType Find(Predicate<AccountType> criteria);

        AccountType GetByKey(string key);
        AccountType GetOrCreateNew(string key);

        IEnumerable<AccountType> ListAvailableAccountTypes();
        IEnumerable<AccountType> ListCurrentlyUsedAccountTypes();
    }
}