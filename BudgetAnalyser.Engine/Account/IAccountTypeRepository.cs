using System;
using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Account
{
    /// <summary>
    /// A repository to store <see cref="Account"/>s. The repository does not allow duplicate entries to be added with the same key.
    /// </summary>
    public interface IAccountTypeRepository
    {
        /// <summary>
        /// Add a new <see cref="Account"/> with the given key. Will not throw if the key already exists in the repository.
        /// </summary>
        /// <param name="key">A unique key.</param>
        /// <param name="instance">The instance to add.</param>
        /// <returns>Either the <param name="instance"/> or if the key already exists in the repository a reference to the existing <see cref="Account"/>.</returns>
        Account Add(string key, Account instance);

        /// <summary>
        /// Search for an existing <see cref="Account"/>.
        /// </summary>
        /// <param name="criteria">A predicate to determine a match.</param>
        /// <returns>The found account or null.</returns>
        Account Find(Predicate<Account> criteria);

        /// <summary>
        /// Retrieve the <see cref="Account"/> for the given key or null if it doesn't exist in the repository.
        /// </summary>
        /// <param name="key">The unique key.</param>
        /// <returns>The found account or null.</returns>
        Account GetByKey(string key);

        /// <summary>
        /// Return a list of all <see cref="Account"/>s in the repository. These are the ones actively being used in the current data loaded.
        /// </summary>
        IEnumerable<Account> ListCurrentlyUsedAccountTypes();
    }
}