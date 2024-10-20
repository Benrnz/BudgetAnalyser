using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.BankAccount
{
    /// <summary>
    ///     A very simple in memory account repository.  Only one of each type is supported at the moment.
    ///     You cannot have two Cheque accounts for example.
    ///     In future this should be enhanced to allow multiple instances of <see cref="Account" /> with different names.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class InMemoryAccountTypeRepository : IAccountTypeRepository
    {
        private readonly ConcurrentDictionary<string, Account> repository = new ConcurrentDictionary<string, Account>(
            8, 5);

        /// <summary>
        ///     Initializes a new instance of the <see cref="InMemoryAccountTypeRepository" /> class.
        /// </summary>
        public InMemoryAccountTypeRepository()
        {
            // Populate the repository with the known list of account I want for now.
            // In a more advanced implementation these would be loaded into the repository when the StatementModel is loaded using 
            // the AccountTypes actively used by it.
            this.repository.TryAdd(AccountTypeRepositoryConstants.Cheque,
                new ChequeAccount(AccountTypeRepositoryConstants.Cheque));
            this.repository.TryAdd(AccountTypeRepositoryConstants.Visa,
                new VisaAccount(AccountTypeRepositoryConstants.Visa));
            this.repository.TryAdd(AccountTypeRepositoryConstants.Savings,
                new SavingsAccount(AccountTypeRepositoryConstants.Savings));
            this.repository.TryAdd(AccountTypeRepositoryConstants.Mastercard,
                new MastercardAccount(AccountTypeRepositoryConstants.Mastercard));
            this.repository.TryAdd(AccountTypeRepositoryConstants.Amex,
                new AmexAccount(AccountTypeRepositoryConstants.Amex));
        }

        /// <summary>
        ///     Add a new <see cref="Account" /> with the given key. Will not throw if the key already exists in the
        ///     repository.
        /// </summary>
        /// <param name="key">A unique key.</param>
        /// <param name="instance">The instance to add.</param>
        /// <returns>
        ///     Either the
        ///     <paramref name="instance" />
        ///     or if the key already exists in the repository a reference to the existing <see cref="Account" />.
        /// </returns>
        public Account Add([NotNull] string key, [NotNull] Account instance)
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            return this.repository.GetOrAdd(key.ToUpperInvariant(), instance);
        }

        /// <summary>
        ///     Search for an existing <see cref="Account" />.
        /// </summary>
        /// <param name="criteria">A predicate to determine a match.</param>
        /// <returns>The found account or null.</returns>
        public Account Find(Predicate<Account> criteria)
        {
            KeyValuePair<string, Account>[] copy = this.repository.ToArray();
            return copy.FirstOrDefault(x => criteria(x.Value)).Value;
        }

        /// <summary>
        ///     Retrieve the <see cref="Account" /> for the given key or null if it doesn't exist in the repository.
        /// </summary>
        /// <param name="key">The unique key.</param>
        /// <returns>The found account or null.</returns>
        public Account GetByKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            Account account;
            if (this.repository.TryGetValue(key.ToUpperInvariant(), out account))
            {
                return account;
            }

            return null;
        }

        /// <summary>
        ///     Return a list of all <see cref="Account" />s in the repository. These are the ones actively being used in the
        ///     current data loaded.
        /// </summary>
        public IEnumerable<Account> ListCurrentlyUsedAccountTypes()
        {
            return this.repository.Values.ToList();
        }
    }
}