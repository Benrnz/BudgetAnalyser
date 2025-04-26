using BudgetAnalyser.Encryption;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.XUnit.TestData;
using ConfuzzleCore;

namespace BudgetAnalyser.Engine.XUnit.TestHarness;

[AutoRegisterWithIoC(Named = StorageConstants.EncryptedInstanceName)]
public class EmbeddedResourceFileReaderWriterEncrypted : IFileReaderWriter
{
    public Func<string, bool>? FileExistsOverride { get; set; }
    public Stream? InputStream { get; set; }
    public Stream OutputStream { get; set; } = new MemoryStream();

    public string Password { get; set; } = TestDataConstants.DemoEncryptedFilePassword;

    public Stream CreateReadableStream(string fileName)
    {
        if (InputStream is null)
        {
            InputStream = GetType().Assembly.GetManifestResourceStream(fileName) ?? throw new FileNotFoundException($"Embedded resource not found: {fileName}");
        }

        return CipherStream.Open(InputStream, Password);
    }

    public Stream CreateWritableStream(string fileName)
    {
        if (OutputStream is null)
        {
            throw new ArgumentNullException(nameof(OutputStream));
        }

        return CipherStream.Create(OutputStream, Password);
    }

    public bool FileExists(string fileName)
    {
        if (FileExistsOverride is not null)
        {
            return FileExistsOverride(fileName);
        }

        using var manifestResourceStream = GetType().Assembly.GetManifestResourceStream(fileName);
        return manifestResourceStream is not null;
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
