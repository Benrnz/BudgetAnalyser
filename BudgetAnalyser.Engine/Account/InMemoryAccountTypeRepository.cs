using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Account
{
    /// <summary>
    ///     A very simple in memory account type repository.  Only one of each type is supported at the moment.
    ///     You cannot have two Cheque accounts for example.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class InMemoryAccountTypeRepository : IAccountTypeRepository
    {
        private readonly AccountType[] referenceAccountTypes =
        {
            new AmexAccount(null),
            new ChequeAccount(null),
            new MastercardAccount(null),
            new SavingsAccount(null),
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

        public IEnumerable<AccountType> ListAvailableAccountTypes()
        {
            // Assumes that you can only have one type of account for each account type class. - Ok for now.
            List<AccountType> availableAccounts = ListCurrentlyUsedAccountTypes().ToList();
            foreach (AccountType refType in this.referenceAccountTypes)
            {
                if (availableAccounts.All(u => u.GetType() != refType.GetType()))
                {
                    string name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(refType.KeyWords[0].ToLowerInvariant());
                    AccountType clone = refType.Clone(name);
                    availableAccounts.Add(clone);
                }
            }

            if (!availableAccounts.Any(a => a is MiscellaneousAccountType))
            {
                availableAccounts.Add(new MiscellaneousAccountType("Miscellaneous"));
            }

            return availableAccounts;
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