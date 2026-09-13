using System.Security;
using System.Text;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Services;

namespace BudgetAnalyser.Encryption;

[AutoRegisterWithIoC(Named = "Encrypted")]
internal class EncryptedLocalDiskReaderWriter(IFileEncryptor fileEncryptor, ICredentialStore credentialStore) : IFileReaderWriter
{
    private readonly ICredentialStore credentialStore = credentialStore ?? throw new ArgumentNullException(nameof(credentialStore));
    private readonly IFileEncryptor fileEncryptor = fileEncryptor ?? throw new ArgumentNullException(nameof(fileEncryptor));

    public Stream CreateWritableStream(string fileName)
    {
        return this.fileEncryptor.CreateWritableEncryptedStream(fileName, RetrievePassword());
    }

    public Stream CreateReadableStream(string fileName)
    {
        var stream = this.fileEncryptor.CreateReadableEncryptedStream(fileName, RetrievePassword());
        if (!stream.CanRead)
        {
            throw new DataFormatException($"Unable to stream data from the file {fileName}.");
        }

        // Check the first 16 bytes of the stream to see if it is a valid encrypted file.
        var checkChunk = new byte[16];
        stream.ReadExactly(checkChunk, 0, 16);
        stream.Position = 0;
        if (!IsValidAlphaNumericWithPunctuation(Encoding.Default.GetString(checkChunk)))
        {
            throw new EncryptionKeyIncorrectException($"Unable to stream any valid data from the file {fileName}. The encryption key is incorrect.");
        }

        return stream;
    }

    /// <summary>
    ///     Checks to see if the File exists.
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
        if (fileName.IsNothing())
        {
            throw new ArgumentNullException(nameof(fileName));
        }

        var password = RetrievePassword();
        var decryptedData = await this.fileEncryptor.LoadEncryptedFileAsync(fileName, password);

        return IsValidAlphaNumericWithPunctuation(decryptedData)
            ? decryptedData
            : throw new EncryptionKeyIncorrectException("The provided encryption credential did not result in a valid decryption result.");
    }

    /// <summary>
    ///     Loads a budget collection xaml file from disk.
    /// </summary>
    /// <param name="fileName">Full path and filename of the file.</param>
    /// <param name="lineCount">The number of lines to load.</param>
    public async Task<string> LoadFirstLinesFromDiskAsync(string fileName, int lineCount)
    {
        if (fileName.IsNothing())
        {
            throw new ArgumentNullException(nameof(fileName));
        }

        var decryptedData = await this.fileEncryptor.LoadFirstLinesFromDiskAsync(fileName, lineCount, RetrievePassword());
        return IsValidAlphaNumericWithPunctuation(decryptedData)
            ? decryptedData
            : throw new EncryptionKeyIncorrectException("The provided encryption credential did not result in a valid decryption result.");
    }

    /// <summary>
    ///     Writes the budget collections to a xaml file on disk.
    /// </summary>
    public async Task WriteToDiskAsync(string fileName, string data)
    {
        if (fileName.IsNothing())
        {
            throw new ArgumentNullException(nameof(fileName));
        }

        if (data.IsNothing())
        {
            throw new ArgumentNullException(nameof(data));
        }

        var password = RetrievePassword();
        await this.fileEncryptor.SaveStringDataToEncryptedFileAsync(fileName, data, password);
    }

    internal bool IsValidAlphaNumericWithPunctuation(string? text)
    {
        if (text is null)
        {
            return false;
        }

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
        if (this.credentialStore.RetrievePasskey() is not SecureString password)
        {
            // This condition should be checked by the UI before calling into the Engine ideally.
            throw new EncryptionKeyNotProvidedException("Attempt to load an encrypted password protected file and no password has been set.");
        }

        return password;
    }
}
