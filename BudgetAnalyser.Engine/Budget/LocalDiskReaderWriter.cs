using System.IO;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Persistence;
using Portable.Xaml;

namespace BudgetAnalyser.Engine.Budget
{
    [AutoRegisterWithIoC(Named = "Unprotected")]
    internal class LocalDiskReaderWriter : IFileReaderWriter
    {
        /// <summary>
        ///     Files the exists.
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
        public async Task<object> LoadFromDiskAsync(string fileName)
        {
            object result = null;
            await Task.Run(() => result = XamlServices.Load(fileName));
            return result;
        }

        /// <summary>
        ///     Writes the budget collections to a xaml file on disk.
        /// </summary>
        public async Task WriteToDiskAsync(string fileName, string data)
        {
            using (var fileStream = new StreamWriter(new FileStream(fileName, FileMode.Create)))
            {
                await fileStream.WriteAsync(data);
            }
        }
    }
}