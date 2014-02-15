using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Account;

namespace BudgetAnalyser.Engine
{
    public interface IAccountTypeRepository
    {
        AccountType Add(string key, AccountType instance);

        AccountType Find(Predicate<AccountType> criteria);

        AccountType GetByKey(string key);
        AccountType GetOrCreateNew(string key);
        IEnumerable<AccountType> ListCurrentlyUsedAccountTypes();
    }
}