using System;
using System.IO;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.UnitTest.Helper;

namespace BudgetAnalyser.Engine.UnitTest.TestHarness
{
    [AutoRegisterWithIoC(Named = "Unprotected")]
    public class EmbeddedResourceFileReaderWriter : IFileReaderWriter
    {
        /// <summary>
        ///     Creates a writable stream to write data into.
        ///     This is an alternative to <see cref="IFileReaderWriter.WriteToDiskAsync" />
        /// </summary>
        /// <param name="fileName">Full path and filename of the file.</param>
        public Stream CreateWritableStream(string fileName)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     Files the exists.
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
            throw new System.NotImplementedException();
        }

        /// <summary>
        ///     Loads a budget collection xaml file from disk.
        /// </summary>
        /// <param name="fileName">Full path and filename of the file.</param>
        public async Task<string> LoadFromDiskAsync(string fileName)
        {
            return await Task.FromResult(EmbeddedResourceHelper.ExtractText(fileName));
        }

        /// <summary>
        ///     Writes the budget collections to a xaml file on disk.
        /// </summary>
        public Task WriteToDiskAsync(string fileName, string data)
        {
            throw new System.NotImplementedException();
        }
    }
}
