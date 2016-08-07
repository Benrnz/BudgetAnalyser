using System.IO;
using System.Threading.Tasks;

namespace BudgetAnalyser.Engine.Persistence
{
    /// <summary>
    ///     An interface for writing and loading data from a persisted file.
    /// </summary>
    public interface IFileReaderWriter
    {
        /// <summary>
        ///     Creates a writable stream to write data into.
        ///     This is an alternative to <see cref="WriteToDiskAsync" />
        /// </summary>
        /// <param name="fileName">Full path and filename of the file.</param>
        Stream CreateWritableStream(string fileName);

        /// <summary>
        ///     Files the exists.
        /// </summary>
        /// <param name="fileName">Full path and filename of the file.</param>
        bool FileExists(string fileName);

        /// <summary>
        ///     Loads a file from disk.
        /// </summary>
        /// <param name="fileName">Full path and filename of the file.</param>
        /// <param name="lineCount">The number of lines to load.</param>
        Task<string> LoadFirstLinesFromDiskAsync(string fileName, int lineCount);

        /// <summary>
        ///     Loads a file from disk.
        /// </summary>
        /// <param name="fileName">Full path and filename of the file.</param>
        Task<string> LoadFromDiskAsync(string fileName);

        /// <summary>
        ///     Writes the data to a file on disk.
        /// </summary>
        Task WriteToDiskAsync(string fileName, string data);
    }
}