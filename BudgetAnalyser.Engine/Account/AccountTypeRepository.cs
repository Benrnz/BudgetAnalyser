using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Account
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class AccountTypeRepository : IAccountTypeRepository
    {
        private readonly AccountType[] referenceAccountTypes =
        {
            new AmexAccount(null),
            new ChequeAccount(null),
            new MastercardAccount(null),
            new VisaAccount(null)
        };

        private readonly ConcurrentDictionary<string, AccountType> repository = new ConcurrentDictionary<string, AccountType>(8, 5);

        public AccountType Add([NotNull] string key, [NotNull] AccountType instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key");
            }

            return this.repository.GetOrAdd(key, instance);
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

        public AccountType GetOrCreateNew(string key)
        {
            return this.repository.GetOrAdd(key, CreateNewAccountType);
        }

        public IEnumerable<AccountType> ListCurrentlyUsedAccountTypes()
        {
            return this.repository.Values.ToList();
        }

        private AccountType CreateNewAccountType(string key)
        {
            string keyIgnoreCase = key.ToUpperInvariant();
            foreach (AccountType referenceType in this.referenceAccountTypes)
            {
                if (referenceType.KeyWords.Any(supportedKeyWord => supportedKeyWord == keyIgnoreCase))
                {
                    return referenceType.Clone(key);
                }
            }

            return new MiscellaneousAccountType(key);
        }
    }
}