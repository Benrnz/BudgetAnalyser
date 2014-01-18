using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BudgetAnalyser.Engine.Account
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class AccountTypeRepository : IAccountTypeRepository
    {
        private readonly ConcurrentDictionary<string, AccountType> repository = new ConcurrentDictionary<string, AccountType>(8, 5);
        private readonly AccountType[] referenceAccountTypes = new AccountType[]
        {
            new AmexAccount(null), 
            new ChequeAccount(null), 
            new MastercardAccount(null), 
            new VisaAccount(null), 
        };
       
        private AccountType CreateNewAccountType(string key)
        {
            var keyIgnoreCase = key.ToUpperInvariant();
            foreach (var referenceType in referenceAccountTypes)
            {
                if (referenceType.KeyWords.Any(supportedKeyWord => supportedKeyWord == keyIgnoreCase))
                {
                    return referenceType.Clone(key);
                }
            }

            return new MiscellaneousAccountType(key);
        }

        public AccountType Add(string key, AccountType instance)
        {
            return this.repository.GetOrAdd(key, instance);
        }

        public AccountType GetOrCreateNew(string key)
        {
            return this.repository.GetOrAdd(key, CreateNewAccountType);
        }

        public AccountType Find(Predicate<AccountType> criteria)
        {
            KeyValuePair<string, AccountType>[] copy = this.repository.ToArray();
            return copy.FirstOrDefault(x => criteria(x.Value)).Value;
        }

        public AccountType GetByKey(string key)
        {
            AccountType accountType;
            if (this.repository.TryGetValue(key, out accountType))
            {
                return accountType;
            }

            return null;
        }

        public IEnumerable<AccountType> List()
        {
            return this.repository.Values.ToList();
        }
    }
}