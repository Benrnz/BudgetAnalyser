using System.Security;

namespace BudgetAnalyser.Encryption;

/// <summary>
///     A utility service for providing encryption functions for files on the local disk.
/// </summary>
public interface IFileEncryptor
{
    /// <summary>
    ///     Creates a readable stream to load and read the data from the disk.
    /// </summary>
    /// <param name="fileName">Full path and filename of the file.</param>
    /// <param name="passphrase">The pass phrase.</param>
    Stream CreateReadableEncryptedStream(string fileName, SecureString passphrase);

    /// <summary>
    ///     Creates a writable stream to write data into.
    /// </summary>
    /// <param name="fileName">Full path and filename of the file.</param>
    /// <param name="passphrase">The pass phrase.</param>
    Stream CreateWritableEncryptedStream(string fileName, SecureString passphrase);

    /// <summary>
    ///     Loads the encrypted file asynchronously and returns its contents as a string (UTF8).
    /// </summary>
    /// <param name="fileName">The path and name of the file.</param>
    /// <param name="passphrase">The pass phrase.</param>
    /// <returns>UTF8 string contents of the file.</returns>
    Task<string> LoadEncryptedFileAsync(string fileName, SecureString passphrase);

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
    Task SaveStringDataToEncryptedFileAsync(string fileName, string data, SecureString passphrase);
}
