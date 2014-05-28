using System;
using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Account
{
    /// <summary>
    /// A repository to store <see cref="AccountType"/>s. The repository does not allow duplicate entries to be added with the same key.
    /// </summary>
    public interface IAccountTypeRepository
    {
        /// <summary>
        /// Add a new <see cref="AccountType"/> with the given key. Will not throw if the key already exists in the repository.
        /// </summary>
        /// <param name="key">A unique key.</param>
        /// <param name="instance">The instance to add.</param>
        /// <returns>Either the <param name="instance"/> or if the key already exists in the repository a reference to the existing <see cref="AccountType"/>.</returns>
        AccountType Add(string key, AccountType instance);

        /// <summary>
        /// Search for an existing <see cref="AccountType"/>.
        /// </summary>
        /// <param name="criteria">A predicate to determine a match.</param>
        /// <returns>The found account type or null.</returns>
        AccountType Find(Predicate<AccountType> criteria);

        /// <summary>
        /// Retrieve the <see cref="AccountType"/> for the given key or null if it doesn't exist in the repository.
        /// </summary>
        /// <param name="key">The unique key.</param>
        /// <returns>The found account type or null.</returns>
        AccountType GetByKey(string key);

        /// <summary>
        /// Retrieve the <see cref="AccountType"/> for the given key or creates a new one if it doesn't exist in the repository.
        /// </summary>
        /// <param name="key">The unique key.</param>
        /// <param name="factory">The delegate to create a new <see cref="AccountType"/>. If not specified, the repository will "guess" which type to create.</param>
        /// <returns>The found account type or a reference to the newly created one.</returns>
        AccountType GetOrCreateNew(string key, Func<string, AccountType> factory = null);

        /// <summary>
        /// Return a list of all <see cref="AccountType"/>s in the repository. These are the ones actively being used in the current data loaded.
        /// </summary>
        IEnumerable<AccountType> ListCurrentlyUsedAccountTypes();
    }
}