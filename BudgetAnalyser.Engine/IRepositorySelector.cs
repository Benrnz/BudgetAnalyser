namespace BudgetAnalyser.Engine
{
    /// <summary>
    /// An interface to describe a class that is able to select an appropriate repository
    /// </summary>
    public interface IRepositorySelector<out TRepository> where TRepository : class
    {
        /// <summary>
        /// Selects a repository implementation based on input parameters.
        /// </summary>
        /// <param name="isEncrypted">if set to <c>true</c> the storage files are encrypted.</param>
        /// <returns>An instance of the repository ready to use.</returns>
        TRepository SelectRepository(bool isEncrypted);
    }
}
