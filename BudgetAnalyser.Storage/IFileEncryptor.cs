using System.Security;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace BudgetAnalyser.Storage
{
    /// <summary>
    ///     A utility service for providing encryption functions for files on the local disk.
    /// </summary>
    public interface IFileEncryptor
    {
        /// <summary>
        ///     Encrypts the source file by copying its contents into a new encrypted destination file.
        ///     The source file remains untouched.
        /// </summary>
        /// <exception cref="System.IO.FileNotFoundException">Will be thrown if the source file does not exist.</exception>
        Task EncryptFileAsync([NotNull] string sourceFile, [NotNull] string destinationFile, [NotNull] SecureString passPhrase);

        /// <summary>
        /// Loads the encrytped file asynchronously and returns its contents as a string (UTF8).
        /// </summary>
        /// <param name="fileName">The path and name of the file.</param>
        /// <param name="passPhrase">The pass phrase.</param>
        /// <returns>UTF8 string contents of the file.</returns>
        Task<string> LoadEncrytpedFileAsync([NotNull] string fileName, [NotNull] SecureString passPhrase);

        /// <summary>
        /// Saves the provided string data (UTF8) into an encrypted file asynchronously.
        /// </summary>
        /// <param name="fileName">The path and name of the file.</param>
        /// <param name="data">The data.</param>
        /// <param name="passPhrase">The pass phrase.</param>
        Task SaveStringDataToEncryptedFileAsync([NotNull] string fileName, [NotNull] string data, [NotNull] SecureString passPhrase);
    }
}