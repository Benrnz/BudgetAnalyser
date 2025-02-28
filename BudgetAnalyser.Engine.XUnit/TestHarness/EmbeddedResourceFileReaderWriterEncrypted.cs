using BudgetAnalyser.Encryption;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.XUnit.TestData;
using ConfuzzleCore;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.XUnit.TestHarness;

[AutoRegisterWithIoC(Named = StorageConstants.EncryptedInstanceName)]
public class EmbeddedResourceFileReaderWriterEncrypted : IFileReaderWriter
{
    public Stream InputStream { get; set; } = null;
    public MemoryStream OutputStream { get; set; } = new();

    public string Password { get; set; } = TestDataConstants.DemoEncryptedFilePassword;

    public Stream CreateReadableStream(string fileName)
    {
        if (InputStream is null)
        {
            throw new ArgumentNullException(nameof(InputStream));
        }

        return CipherStream.Open(InputStream, Password);
    }

    public Stream CreateWritableStream(string fileName)
    {
        if (OutputStream is null)
        {
            throw new ArgumentNullException(nameof(InputStream));
        }

        return CipherStream.Create(OutputStream, Password);
    }

    public bool FileExists(string fileName)
    {
        throw new NotImplementedException();
    }

    public Task<string> LoadFirstLinesFromDiskAsync(string fileName, int lineCount)
    {
        throw new NotImplementedException();
    }

    public Task<string> LoadFromDiskAsync(string fileName)
    {
        throw new NotImplementedException();
    }

    public Task WriteToDiskAsync(string fileName, string data)
    {
        throw new NotImplementedException();
    }
}
