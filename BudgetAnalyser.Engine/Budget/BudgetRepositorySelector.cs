using System;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Budget
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    internal class BudgetRepositorySelector : IRepositorySelector<IBudgetRepository>
    {
        private readonly IBudgetRepository encryptedRepository;
        private readonly IBudgetRepository unprotectedRepository;

        public BudgetRepositorySelector(
            [NotNull] IBudgetRepository unprotectedRepository,
            [NotNull] IBudgetRepository encryptedRepository)
        {
            if (unprotectedRepository == null) throw new ArgumentNullException(nameof(unprotectedRepository));
            if (encryptedRepository == null) throw new ArgumentNullException(nameof(encryptedRepository));
            this.unprotectedRepository = unprotectedRepository;
            this.encryptedRepository = encryptedRepository;
        }

        /// <summary>
        ///     Selects a repository implementation based on input parameters.
        /// </summary>
        /// <param name="isEncrypted">if set to <c>true</c> the storage files are encrypted.</param>
        /// <returns>An instance of the repository ready to use.</returns>
        public IBudgetRepository SelectRepository(bool isEncrypted)
        {
            return isEncrypted ? this.encryptedRepository : this.unprotectedRepository;
        }
    }
}