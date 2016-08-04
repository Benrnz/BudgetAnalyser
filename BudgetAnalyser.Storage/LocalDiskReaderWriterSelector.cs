using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Persistence;
using JetBrains.Annotations;

namespace BudgetAnalyser.Encryption
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    internal class LocalDiskReaderWriterSelector : IReaderWriterSelector
    {
        private readonly IFileReaderWriter encryptedReaderWriter;
        private readonly IFileReaderWriter unprotectedRaderWriter;

        public LocalDiskReaderWriterSelector([NotNull] IEnumerable<IFileReaderWriter> allReaderWriters)
        {
            if (allReaderWriters == null) throw new ArgumentNullException(nameof(allReaderWriters));

            var fileReaderWriters = allReaderWriters.ToList();
            this.encryptedReaderWriter = DefaultIoCRegistrations.GetNamedInstance(fileReaderWriters, StorageConstants.EncryptedInstanceName);
            this.unprotectedRaderWriter = DefaultIoCRegistrations.GetNamedInstance(fileReaderWriters, StorageConstants.UnprotectedInstanceName);
        }

        /// <summary>
        ///     Selects a repository implementation based on input parameters.
        /// </summary>
        /// <param name="isEncrypted">if set to <c>true</c> the storage files are encrypted.</param>
        /// <returns>An instance of the repository ready to use.</returns>
        public IFileReaderWriter SelectReaderWriter(bool isEncrypted)
        {
            return isEncrypted ? this.encryptedReaderWriter : this.unprotectedRaderWriter;
        }
    }
}