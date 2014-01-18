using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Account;

namespace BudgetAnalyser.Engine
{
    public interface IAccountTypeRepository
    {
        AccountType Add(string key, AccountType instance);

        AccountType GetOrCreateNew(string key);
        AccountType Find(Predicate<AccountType> criteria);

        AccountType GetByKey(string key);
        IEnumerable<AccountType> List();
    }
}