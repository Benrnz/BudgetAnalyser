using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Services;
using JetBrains.Annotations;

namespace BudgetAnalyser.Encryption
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

        public Stream CreateWritableStream(string fileName)
        {
            return this.fileEncryptor.CreateWritableEncryptedStream(fileName, RetrievePassword());
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
        public async Task<string> LoadFromDiskAsync(string fileName)
        {
            if (fileName.IsNothing()) throw new ArgumentNullException(nameof(fileName));
            var password = RetrievePassword();
            var decryptedData = await this.fileEncryptor.LoadEncryptedFileAsync(fileName, password);

            if (IsValidAlphaNumericWithPunctuation(decryptedData))
            {
                return decryptedData;
            }

            throw new EncryptionKeyIncorrectException("The provided encryption credential did not result in a valid decryption result.");
        }

        /// <summary>
        ///     Loads a budget collection xaml file from disk.
        /// </summary>
        /// <param name="fileName">Full path and filename of the file.</param>
        /// <param name="lineCount">The number of lines to load.</param>
        public async Task<string> LoadFirstLinesFromDiskAsync(string fileName, int lineCount)
        {
            if (fileName.IsNothing()) throw new ArgumentNullException(nameof(fileName));
            var decryptedData = await this.fileEncryptor.LoadFirstLinesFromDiskAsync(fileName, lineCount, RetrievePassword());
            if (IsValidAlphaNumericWithPunctuation(decryptedData))
            {
                return decryptedData;
            }

            throw new EncryptionKeyIncorrectException("The provided encryption credential did not result in a valid decryption result.");
        }

        /// <summary>
        ///     Writes the budget collections to a xaml file on disk.
        /// </summary>
        public async Task WriteToDiskAsync(string fileName, string data)
        {
            if (fileName.IsNothing()) throw new ArgumentNullException(nameof(fileName));
            if (data.IsNothing()) throw new ArgumentNullException(nameof(data));

            var password = RetrievePassword();
            await this.fileEncryptor.SaveStringDataToEncryptedFileAsync(fileName, data, password);
        }

        internal bool IsValidAlphaNumericWithPunctuation(string text)
        {
            if (text == null) return false;
            var valid = text.ToCharArray().Take(16).All(IsValidUtf8AlphaNumericWithPunctuation);
            return valid;
        }

        private bool IsValidUtf8AlphaNumericWithPunctuation(char c)
        {
            // 0x0000007e is Tilde which is considered valid.
            // 0x00000000 is a null character which is invalid.
            // Everything beyond 0x0000007f is considered invalid as it is not plain text.
            var valid = c < 0x0000007f && c > 0x00000000;
            return valid;
        }

        private SecureString RetrievePassword()
        {
            var password = this.credentialStore.RetrievePasskey() as SecureString;

            if (password == null)
            {
                // This condition should be checked by the UI before calling into the Engine ideally.
                throw new EncryptionKeyNotProvidedException("Attempt to load an encrypted password protected file and no password has been set.");
            }

            return password;
        }
    }
}