using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Persistence;
using JetBrains.Annotations;
using Portable.Xaml;

namespace BudgetAnalyser.Storage
{
    [AutoRegisterWithIoC(Named = "Encrypted")]
    internal class EncryptedLocalDiskReaderWriter : IFileReaderWriter
    {
        private readonly IFileEncryptor fileEncryptor;
        private readonly ICredentialStore credentialStore;

        public EncryptedLocalDiskReaderWriter([NotNull] IFileEncryptor fileEncryptor, [NotNull] ICredentialStore credentialStore)
        {
            if (fileEncryptor == null) throw new ArgumentNullException(nameof(fileEncryptor));
            if (credentialStore == null) throw new ArgumentNullException(nameof(credentialStore));
            this.fileEncryptor = fileEncryptor;
            this.credentialStore = credentialStore;
        }

        /// <summary>
        ///     Files the exists.
        /// </summary>
        /// <param name="fileName">Full path and filename of the file.</param>
        public bool FileExists(string fileName)
        {
            return File.Exists(fileName);
        }

        /// <summary>
        ///     Loads a budget collection xaml file from disk.
        /// </summary>
        /// <param name="fileName">Full path and filename of the file.</param>
        public async Task<object> LoadFromDiskAsync(string fileName)
        {
            var password = RetrievePassword();
            Debug.WriteLine($"Password is: {CredentialStore.SecureStringToString(password)}"); // TODO remove this

            if (fileName.IsNothing()) throw new ArgumentNullException(nameof(fileName));
            var decryptedData = await this.fileEncryptor.LoadEncryptedFileAsync(fileName, password);
            return XamlServices.Parse(decryptedData);
        }

        /// <summary>
        ///     Writes the budget collections to a xaml file on disk.
        /// </summary>
        public async Task WriteToDiskAsync(string fileName, string data)
        {
            if (fileName.IsNothing()) throw new ArgumentNullException(nameof(fileName));
            if (data.IsNothing()) throw new ArgumentNullException(nameof(data));

            var password = RetrievePassword();
            Debug.WriteLine($"Password is: {CredentialStore.SecureStringToString(password)}"); // TODO remove this

            await this.fileEncryptor.SaveStringDataToEncryptedFileAsync(fileName, data, password);
        }

        private SecureString RetrievePassword()
        {
            var password = this.credentialStore.RetrievePasskey() as SecureString;

            if (password == null)
            {
                throw new SecurityException("Attempt to load an encrypted password protected file and no password has been set.");
            }

            return password;
        }
    }
}
