using System;
using System.Threading.Tasks;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using JetBrains.Annotations;
using Portable.Xaml;
using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Storage
{
    public class EncryptedXamlOnDiskBudgetRepository : XamlOnDiskBudgetRepository
    {
        private readonly ICredentialStore credentialStore;
        private readonly IFileEncrypter fileEncrypter;

        /// <summary>
        ///     Initializes a new instance of the <see cref="XamlOnDiskBudgetRepository" /> class.
        /// </summary>
        public EncryptedXamlOnDiskBudgetRepository(
            [NotNull] IBudgetBucketRepository bucketRepository,
            [NotNull] IDtoMapper<BudgetCollectionDto, BudgetCollection> mapper,
            [NotNull] IFileEncrypter fileEncrypter,
            [NotNull] ICredentialStore credentialStore)
            : base(bucketRepository, mapper)
        {
            if (bucketRepository == null) throw new ArgumentNullException(nameof(bucketRepository));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));
            if (fileEncrypter == null) throw new ArgumentNullException(nameof(fileEncrypter));
            this.fileEncrypter = fileEncrypter;
            this.credentialStore = credentialStore;
        }

        /// <summary>
        ///     Loads a budget collection xaml file from disk.
        /// </summary>
        /// <param name="fileName">Full path and filename of the file.</param>
        protected override async Task<object> LoadFromDisk([NotNull] string fileName)
        {
            if (fileName.IsNothing()) throw new ArgumentNullException(nameof(fileName));
            var decryptedData = await this.fileEncrypter.LoadEncryptedFileAsync(fileName, this.credentialStore.RetrievePassword());
            return XamlServices.Parse(decryptedData);
        }


        /// <summary>
        ///     Writes the budget collections to a xaml file on disk.
        /// </summary>
        protected override async Task WriteToDisk(string fileName, string data)
        {
            if (fileName.IsNothing()) throw new ArgumentNullException(nameof(fileName));
            if (data.IsNothing()) throw new ArgumentNullException(nameof(data));

            // Remove this when confidence is high:
            await base.WriteToDisk(fileName + ".backup", data);
            await this.fileEncrypter.SaveStringDataToEncryptedFileAsync(fileName, data, this.credentialStore.RetrievePassword());
        }
    }
}