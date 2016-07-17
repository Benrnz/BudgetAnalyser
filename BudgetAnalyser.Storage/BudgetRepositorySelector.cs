using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using JetBrains.Annotations;

namespace BudgetAnalyser.Storage
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    internal class BudgetRepositorySelector : IRepositorySelector<IBudgetRepository>
    {
        private readonly IBudgetRepository encryptedRepository;
        private readonly IBudgetRepository unprotectedRepository;

        public BudgetRepositorySelector([NotNull] IEnumerable<IBudgetRepository> repositories)
        {
            if (repositories == null) throw new ArgumentNullException(nameof(repositories));

            var listOfRepos = repositories.ToList();
            this.encryptedRepository = DefaultIoCRegistrations.GetNamedInstance(listOfRepos, StorageConstants.EncryptedInstanceName);
            this.unprotectedRepository = DefaultIoCRegistrations.GetNamedInstance(listOfRepos, StorageConstants.UnprotectedInstanceName);
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