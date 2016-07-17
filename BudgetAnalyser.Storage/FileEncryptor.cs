using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using BudgetAnalyser.Engine;
using Confuzzle.Core;

namespace BudgetAnalyser.Storage
{
    /// <summary>
    ///     A utility class for encrypting files on the local disk.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class FileEncryptor : IFileEncryptor
    {
        /// <summary>
        ///     Encrypts the source file by copying its contents into a new encrypted destination file.
        ///     The source file remains untouched.
        /// </summary>
        /// <exception cref="System.IO.FileNotFoundException">Will be thrown if the source file does not exist.</exception>
        public async Task EncryptFileAsync(string sourceFile, string destinationFile, SecureString passphrase)
        {
            if (sourceFile.IsNothing()) throw new ArgumentNullException(nameof(sourceFile));
            if (destinationFile.IsNothing()) throw new ArgumentNullException(nameof(destinationFile));
            if (passphrase == null) throw new ArgumentNullException(nameof(passphrase));

            if (!FileExists(sourceFile))
            {
                throw new FileNotFoundException(sourceFile);
            }

            using (var inputStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                using (var outputStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, true))
                {
                    using (var cryptoStream = CipherStream.Create(outputStream, SecureStringToString(passphrase)))
                    {
                        // Copy the contents of the input stream into the output stream (file) and in doing so encrypt it.
                        await inputStream.CopyToAsync(cryptoStream);
                    }
                }
            }
        }

        /// <summary>
        ///     Loads the encrytped file asynchronously and returns its contents as a string (UTF8).
        /// </summary>
        /// <param name="fileName">The path and name of the file.</param>
        /// <param name="passphrase">The pass phrase.</param>
        /// <returns>UTF8 string contents of the file.</returns>
        public async Task<string> LoadEncryptedFileAsync(string fileName, SecureString passphrase)
        {
            string decryptedData;
            using (var inputStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                using (var outputStream = new MemoryStream())
                {
                    using (var cryptoStream = CipherStream.Open(inputStream, SecureStringToString(passphrase)))
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

            return decryptedData;
        }

        /// <summary>
        ///     Saves the provided string data into an encrypted file asynchronously.
        /// </summary>
        /// <param name="fileName">The path and name of the file.</param>
        /// <param name="data">The data.</param>
        /// <param name="passphrase">The pass phrase.</param>
        public async Task SaveStringDataToEncryptedFileAsync(string fileName, string data, SecureString passphrase)
        {
            using (var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                using (var outputStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, true))
                {
                    using (var cryptoStream = CipherStream.Create(outputStream, SecureStringToString(passphrase)))
                    {
                        await inputStream.CopyToAsync(cryptoStream);
                    }
                }
            }
        }

        protected virtual bool FileExists(string fileName)
        {
            return File.Exists(fileName);
        }

        private static string SecureStringToString(SecureString value)
        {
            var valuePtr = IntPtr.Zero;
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