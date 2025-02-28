using System.Reflection;
using BudgetAnalyser.Encryption;
using BudgetAnalyser.Engine.Persistence;
using Rees.UnitTestUtilities;

namespace BudgetAnalyser.Engine.XUnit.TestHarness;

[AutoRegisterWithIoC(Named = StorageConstants.UnprotectedInstanceName)]
public class EmbeddedResourceFileReaderWriter : IFileReaderWriter
{
    /// <summary>
    ///     Creates a writable stream to write data into. This is an alternative to <see cref="IFileReaderWriter.WriteToDiskAsync" />
    /// </summary>
    /// <param name="fileName">Full path and filename of the file.</param>
    public Stream CreateWritableStream(string fileName)
    {
        return new MemoryStream();
    }

    public Stream CreateReadableStream(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream(fileName) ?? throw new FileNotFoundException($"Embedded resource not found: {fileName}");
        return stream;
    }

    /// <summary>
    ///     Returns true if the file exists in local storage.
    /// </summary>
    /// <param name="fileName">Full path and filename of the file.</param>
    public bool FileExists(string fileName)
    {
        return true;
    }

    /// <summary>
    ///     Loads a file from disk.
    /// </summary>
    /// <param name="fileName">Full path and filename of the file.</param>
    /// <param name="lineCount">The number of lines to load.</param>
    public Task<string> LoadFirstLinesFromDiskAsync(string fileName, int lineCount)
    {
        throw new NotImplementedException("Implement these when required");
    }

    /// <summary>
    ///     Loads a budget collection xaml file from disk.
    /// </summary>
    /// <param name="fileName">Full path and filename of the file.</param>
    public async Task<string> LoadFromDiskAsync(string fileName)
    {
        return await Task.FromResult(GetType().Assembly.ExtractEmbeddedResourceAsText(fileName));
    }

    /// <summary>
    ///     Writes the budget collections to a xaml file on disk.
    /// </summary>
    public Task WriteToDiskAsync(string fileName, string data)
    {
        throw new NotImplementedException();
    }
}
