using System.Text;

namespace BudgetAnalyser.Engine.Persistence;

/// <summary>
///     Reads and writes to local plain text files on disk.
/// </summary>
[AutoRegisterWithIoC(Named = "Unprotected")]
internal class LocalDiskReaderWriter : IFileReaderWriter
{
    /// <summary>
    ///     Creates a writable stream to write data into.
    ///     This is an alternative to <see cref="IFileReaderWriter.WriteToDiskAsync" />
    /// </summary>
    /// <param name="fileName">Full path and filename of the file.</param>
    public Stream CreateWritableStream(string fileName)
    {
        return new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous);
    }

    public bool FileExists(string fileName)
    {
        return fileName.IsNothing() ? throw new ArgumentNullException(nameof(fileName)) : File.Exists(fileName);
    }

    public async Task<string> LoadFromDiskAsync(string fileName)
    {
        if (fileName.IsNothing())
        {
            throw new ArgumentNullException(nameof(fileName));
        }

        using var reader = File.OpenText(fileName);
        try
        {
            var allText = await reader.ReadToEndAsync();
            return allText;
        }
        catch (IOException)
        {
            return string.Empty;
        }
    }

    public async Task<string> LoadFirstLinesFromDiskAsync(string fileName, int lineCount)
    {
        if (fileName.IsNothing())
        {
            throw new ArgumentNullException(nameof(fileName));
        }

        using var reader = File.OpenText(fileName);
        try
        {
            var builder = new StringBuilder();
            for (var index = 0; index < lineCount; index++)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line))
                {
                    break;
                }

                builder.AppendLine(line);
            }

            return builder.ToString();
        }
        catch (IOException)
        {
            return string.Empty;
        }
    }

    public async Task WriteToDiskAsync(string fileName, string data)
    {
        if (fileName.IsNothing())
        {
            throw new ArgumentNullException(nameof(fileName));
        }

        await using var fileStream = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write, 4096, true));
        await fileStream.WriteAsync(data);
    }
}
