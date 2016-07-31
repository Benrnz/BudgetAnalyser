using System.Threading.Tasks;

namespace BudgetAnalyser.Engine.Persistence
{
    /// <summary>
    ///     An interface for writing and loading data from a persisted file.
    /// </summary>
    public interface IFileReaderWriter
    {
        /// <summary>
        ///     Files the exists.
        /// </summary>
        /// <param name="fileName">Full path and filename of the file.</param>
        bool FileExists(string fileName);

        /// <summary>
        ///     Loads a budget collection xaml file from disk.
        /// </summary>
        /// <param name="fileName">Full path and filename of the file.</param>
        Task<object> LoadFromDiskAsync(string fileName);

        /// <summary>
        ///     Writes the budget collections to a xaml file on disk.
        /// </summary>
        Task WriteToDiskAsync(string fileName, string data);
    }
}