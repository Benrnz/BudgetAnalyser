using System;
using System.IO;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using BudgetAnalyser.Engine;
using ConfuzzleCore;

namespace BudgetAnalyser.Encryption
{
    /// <summary>
    ///     A utility class for encrypting files on the local disk.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class FileEncryptor : IFileEncryptor
    {
        /// <summary>
        ///     Encrypts the source file by copying its contents into a new encrypted destination file.
        ///     The source file remains untouched.
        /// </summary>
        /// <exception cref="System.IO.FileNotFoundException">Will be thrown if the source file does not exist.</exception>
        public async Task EncryptFileAsync(string sourceFile, string destinationFile, SecureString passphrase)
        {
            if (sourceFile.IsNothing()) throw new ArgumentNullException(nameof(sourceFile));
            if (destinationFile.IsNothing()) throw new ArgumentNullException(nameof(destinationFile));
            if (passphrase == null) throw new ArgumentNullException(nameof(passphrase));

            if (!FileExists(sourceFile))
            {
                throw new FileNotFoundException(sourceFile);
            }

            await Confuzzle.EncryptFile(sourceFile).WithPassword(passphrase).IntoFile(destinationFile);
        }

        /// <summary>
        ///     Loads the encrytped file asynchronously and returns its contents as a string (UTF8).
        /// </summary>
        /// <param name="fileName">The path and name of the file.</param>
        /// <param name="passphrase">The pass phrase.</param>
        /// <returns>UTF8 string contents of the file.</returns>
        public async Task<string> LoadEncryptedFileAsync(string fileName, SecureString passphrase)
        {
            return await Confuzzle.DecryptFile(fileName).WithPassword(passphrase).IntoString();
        }

        /// <summary>
        ///     Saves the provided string data into an encrypted file asynchronously.
        /// </summary>
        /// <param name="fileName">The path and name of the file.</param>
        /// <param name="data">The data.</param>
        /// <param name="passphrase">The pass phrase.</param>
        public async Task SaveStringDataToEncryptedFileAsync(string fileName, string data, SecureString passphrase)
        {
            await Confuzzle.EncryptString(data).WithPassword(passphrase).IntoFile(fileName);
        }

        protected virtual bool FileExists(string fileName)
        {
            return File.Exists(fileName);
        }
    }
}