using System.IO;
using System.Security;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace BudgetAnalyser.Encryption
{
    /// <summary>
    ///     A utility service for providing encryption functions for files on the local disk.
    /// </summary>
    public interface IFileEncryptor
    {
        /// <summary>
        ///     Creates a writable stream to write data into.
        /// </summary>
        /// <param name="fileName">Full path and filename of the file.</param>
        /// <param name="passphrase">The pass phrase.</param>
        Stream CreateWritableEncryptedStream(string fileName, SecureString passphrase);

        /// <summary>
        ///     Encrypts the source file by copying its contents into a new encrypted destination file.
        ///     The source file remains untouched.
        /// </summary>
        /// <param name="destinationFile">The file to write encrypted data into.</param>
        /// <param name="passphrase">The pass phrase.</param>
        /// <param name="sourceFile">The plain text source file.</param>
        /// <exception cref="System.IO.FileNotFoundException">Will be thrown if the source file does not exist.</exception>
        Task EncryptFileAsync([NotNull] string sourceFile, [NotNull] string destinationFile, [NotNull] SecureString passphrase);

        /// <summary>
        ///     Loads the encrypted file asynchronously and returns its contents as a string (UTF8).
        /// </summary>
        /// <param name="fileName">The path and name of the file.</param>
        /// <param name="passphrase">The pass phrase.</param>
        /// <returns>UTF8 string contents of the file.</returns>
        Task<string> LoadEncryptedFileAsync([NotNull] string fileName, [NotNull] SecureString passphrase);

        /// <summary>
        ///     Loads a file from disk and returns the specified number of lines.
        /// </summary>
        /// <param name="fileName">Full path and filename of the file.</param>
        /// <param name="lineCount">The number of lines to load.</param>
        /// <param name="passphrase">The pass phrase.</param>
        /// <returns>UTF8 string contents of the file.</returns>
        Task<string> LoadFirstLinesFromDiskAsync(string fileName, int lineCount, SecureString passphrase);

        /// <summary>
        ///     Saves the provided string data (UTF8) into an encrypted file asynchronously.
        /// </summary>
        /// <param name="fileName">The path and name of the file.</param>
        /// <param name="data">The data.</param>
        /// <param name="passphrase">The pass phrase.</param>
        Task SaveStringDataToEncryptedFileAsync([NotNull] string fileName, [NotNull] string data, [NotNull] SecureString passphrase);
    }
}