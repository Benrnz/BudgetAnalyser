using System.Threading.Tasks;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.UnitTest.Helper;

namespace BudgetAnalyser.Engine.UnitTest.TestHarness
{
    [AutoRegisterWithIoC(Named = "Unprotected")]
    public class EmbeddedResourceFileReaderWriter : IFileReaderWriter
    {
        /// <summary>
        ///     Files the exists.
        /// </summary>
        /// <param name="fileName">Full path and filename of the file.</param>
        public bool FileExists(string fileName)
        {
            return true;
        }

        /// <summary>
        ///     Loads a budget collection xaml file from disk.
        /// </summary>
        /// <param name="fileName">Full path and filename of the file.</param>
        public async Task<object> LoadFromDiskAsync(string fileName)
        {
            return await Task.FromResult(EmbeddedResourceHelper.ExtractXaml<object>(fileName, false));
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
