using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Services;
using JetBrains.Annotations;
using Portable.Xaml;

namespace BudgetAnalyser.Storage
{
    [AutoRegisterWithIoC(Named = "Encrypted")]
    internal class EncryptedLocalDiskReaderWriter : IFileReaderWriter
    {
        private readonly ICredentialStore credentialStore;
        private readonly IFileEncryptor fileEncryptor;

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

            if (fileName.IsNothing()) throw new ArgumentNullException(nameof(fileName));
            var decryptedData = await this.fileEncryptor.LoadEncryptedFileAsync(fileName, password);

            var validText = true;
            int characterCount = 0;

            if (validText)
            {
                return XamlServices.Parse(decryptedData);
            }

            throw new EncryptionKeyIncorrectException("The provided encryption credential did not result in a valid decryption result.");
        }

        public bool IsValidAlphaNumericWithPunctuation(string text)
        {
            if (text == null) return false;
            var valid = text.ToCharArray().Take(16).All(IsValidAlphaNumericWithPunctuation);
            return valid;
        }

        private bool IsValidAlphaNumericWithPunctuation(char c)
        {
            var valid = c < 0x0000007f && c > 0x00000000;
            return valid;
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