namespace BudgetAnalyser.Engine.BankAccount;

/// <summary>
///     A repository to store <see cref="Account" />s. The repository does not allow duplicate entries to be added with the
///     same key.
/// </summary>
public interface IAccountTypeRepository
{
    /// <summary>
    ///     Retrieve the <see cref="Account" /> for the given key or null if it doesn't exist in the repository.
    /// </summary>
    /// <param name="key">The unique key.</param>
    /// <returns>The found account or null.</returns>
    Account? GetByKey(string key);

    /// <summary>
    ///     Return a list of all <see cref="Account" />s in the repository. These are the ones actively being used in the
    ///     current data loaded.
    /// </summary>
    IEnumerable<Account> ListCurrentlyUsedAccountTypes();
}
