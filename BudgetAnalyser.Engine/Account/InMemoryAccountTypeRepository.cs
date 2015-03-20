using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Account
{
    /// <summary>
    ///     A very simple in memory account type repository.  Only one of each type is supported at the moment.
    ///     You cannot have two Cheque accounts for example.
    ///     In future this should be enhanced to allow multiple instances of <see cref="AccountType" /> with different names.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class InMemoryAccountTypeRepository : IAccountTypeRepository
    {
        private readonly ConcurrentDictionary<string, AccountType> repository = new ConcurrentDictionary<string, AccountType>(8, 5);

        public InMemoryAccountTypeRepository()
        {
            // Populate the repository with the known list of account types I want for now.
            // In a more advanced implementation these would be loaded into the repository when the StatementModel is loaded using 
            // the AccountTypes actively used by it.
            this.repository.TryAdd(AccountTypeRepositoryConstants.Cheque, new ChequeAccount(AccountTypeRepositoryConstants.Cheque));
            this.repository.TryAdd(AccountTypeRepositoryConstants.Visa, new VisaAccount(AccountTypeRepositoryConstants.Visa));
            this.repository.TryAdd(AccountTypeRepositoryConstants.Savings, new SavingsAccount(AccountTypeRepositoryConstants.Savings));
            this.repository.TryAdd(AccountTypeRepositoryConstants.Mastercard, new MastercardAccount(AccountTypeRepositoryConstants.Mastercard));
            this.repository.TryAdd(AccountTypeRepositoryConstants.Amex, new AmexAccount(AccountTypeRepositoryConstants.Amex));
        }

        /// <summary>
        ///     Add a new <see cref="AccountType" /> with the given key. Will not throw if the key already exists in the
        ///     repository.
        /// </summary>
        /// <param name="key">A unique key.</param>
        /// <param name="instance">The instance to add.</param>
        /// <returns>
        ///     Either the
        ///     <param name="instance" />
        ///     or if the key already exists in the repository a reference to the existing <see cref="AccountType" />.
        /// </returns>
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

            return this.repository.GetOrAdd(key.ToUpperInvariant(), instance);
        }

        /// <summary>
        ///     Search for an existing <see cref="AccountType" />.
        /// </summary>
        /// <param name="criteria">A predicate to determine a match.</param>
        /// <returns>The found account type or null.</returns>
        public AccountType Find(Predicate<AccountType> criteria)
        {
            KeyValuePair<string, AccountType>[] copy = this.repository.ToArray();
            return copy.FirstOrDefault(x => criteria(x.Value)).Value;
        }

        /// <summary>
        ///     Retrieve the <see cref="AccountType" /> for the given key or null if it doesn't exist in the repository.
        /// </summary>
        /// <param name="key">The unique key.</param>
        /// <returns>The found account type or null.</returns>
        public AccountType GetByKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            AccountType accountType;
            if (this.repository.TryGetValue(key.ToUpperInvariant(), out accountType))
            {
                return accountType;
            }

            return null;
        }

        /// <summary>
        ///     Return a list of all <see cref="AccountType" />s in the repository. These are the ones actively being used in the
        ///     current data loaded.
        /// </summary>
        public IEnumerable<AccountType> ListCurrentlyUsedAccountTypes()
        {
            return this.repository.Values.ToList();
        }
    }
}