using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using Confuzzle.Core;
using Portable.Xaml;
using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Storage
{
    public class EncryptedXamlOnDiskBudgetRepository : XamlOnDiskBudgetRepository
    {
        private readonly SecureString passPhrase;

        /// <summary>
        ///     Initializes a new instance of the <see cref="XamlOnDiskBudgetRepository" /> class.
        /// </summary>
        public EncryptedXamlOnDiskBudgetRepository(IBudgetBucketRepository bucketRepository, IDtoMapper<BudgetCollectionDto, BudgetCollection> mapper, SecureString passPhrase)
            : base(bucketRepository, mapper)
        {
            this.passPhrase = passPhrase;
        }

        /// <summary>
        ///     Loads a budget collection xaml file from disk.
        /// </summary>
        /// <param name="fileName">Full path and filename of the file.</param>
        protected override async Task<object> LoadFromDisk(string fileName)
        {
            string decryptedData = null;
            using (var inputStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
            {
                using (var outputStream = new MemoryStream())
                {
                    using (var cryptoStream = CipherStream.Open(inputStream, SecureStringToString(this.passPhrase)))
                    {
                        await cryptoStream.CopyToAsync(outputStream);
                    }

                    outputStream.Position = 0;
                    using (var reader = new StreamReader(outputStream))
                    {
                        decryptedData = await reader.ReadToEndAsync();
                    }
                }
            }

            return XamlServices.Parse(decryptedData);
        }

        /// <summary>
        ///     Writes the budget collections to a xaml file on disk.
        /// </summary>
        protected override async Task WriteToDisk(string fileName, string data)
        {
            // Remove this when confidence is high:
            await base.WriteToDisk(fileName + ".backup", data);

            using (var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                using (var outputStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 4096, useAsync: true))
                {
                    using (var cryptoStream = CipherStream.Create(outputStream, SecureStringToString(this.passPhrase)))
                    {
                        await inputStream.CopyToAsync(cryptoStream);
                    }
                }
            }
        }

        private string SecureStringToString(SecureString value)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }
    }
}
