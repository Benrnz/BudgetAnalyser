using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using BudgetAnalyser.Engine;
using ConfuzzleCore;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;

namespace BudgetAnalyser.Encryption
{
    /// <summary>
    ///     A utility class for encrypting files on the local disk.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    internal class FileEncryptor : IFileEncryptor
    {
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Output stream is disposed by consumer when CipherStream is disposed")]
        public Stream CreateWritableEncryptedStream(string fileName, SecureString passphrase)
        {
            var outputStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 4096, FileOptions.Asynchronous);
            return CipherStream.Create(outputStream, SecureStringCredentialStore.SecureStringToString(passphrase));
        }

        public async Task EncryptFileAsync(string sourceFile, string destinationFile, SecureString passphrase)
        {
            if (sourceFile.IsNothing()) throw new ArgumentNullException(nameof(sourceFile));
            if (destinationFile.IsNothing()) throw new ArgumentNullException(nameof(destinationFile));
            if (passphrase == null) throw new ArgumentNullException(nameof(passphrase));

            if (!FileExists(sourceFile))
            {
                throw new FileNotFoundException(sourceFile);
            }

            await Confuzzle.EncryptFile(sourceFile).WithPassword(passphrase).IntoFile(destinationFile);
        }

        public async Task<string> LoadEncryptedFileAsync(string fileName, SecureString passphrase)
        {
            return await Confuzzle.DecryptFile(fileName).WithPassword(passphrase).IntoString();
        }

        public async Task<string> LoadFirstLinesFromDiskAsync(string fileName, int lineCount, SecureString passphrase)
        {
            using (var inputStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                using (var outputStream = new MemoryStream())
                {
                    var password = SecureStringCredentialStore.SecureStringToString(passphrase);
                    using (var cryptoStream = CipherStream.Open(inputStream, password))
                    {
                        while (cryptoStream.CanRead)
                        {
                            var buffer = new byte[4096];
                            await cryptoStream.ReadAsync(buffer, 0, 4096);
                            await outputStream.WriteAsync(buffer, 0, 4096);
                            char[] chunk = Encoding.UTF8.GetChars(buffer);
                            var occurances = chunk.Count(c => c == '\n');
                            if (occurances >= lineCount) break;
                        }
                    }

                    outputStream.Position = 0;
                    using (var reader = new StreamReader(outputStream))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
            }
        }

        public async Task SaveStringDataToEncryptedFileAsync(string fileName, string data, SecureString passphrase)
        {
            await Confuzzle.EncryptString(data).WithPassword(passphrase).IntoFile(fileName);
        }

        protected virtual bool FileExists(string fileName)
        {
            return File.Exists(fileName);
        }
    }
}