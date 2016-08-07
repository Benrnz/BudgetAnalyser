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
            var outputStream = new MyFileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 4096, FileOptions.Asynchronous);
            // var outputStream = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Read);
            return CipherStream.Create(outputStream, CredentialStore.SecureStringToString(passphrase));
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
                    var password = CredentialStore.SecureStringToString(passphrase);
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

    internal class MyFileStream : FileStream
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class with the specified path and
        ///     creation mode.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate. </param>
        /// <param name="mode">A constant that determines how to open or create the file. </param>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="path" /> is an empty string (""), contains only white space, or contains one or more invalid
        ///     characters. -or-<paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an
        ///     NTFS environment.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///     <paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS
        ///     environment.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="path" /> is null.
        /// </exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        /// <exception cref="T:System.IO.FileNotFoundException">
        ///     The file cannot be found, such as when <paramref name="mode" /> is
        ///     FileMode.Truncate or FileMode.Open, and the file specified by <paramref name="path" /> does not exist. The file
        ///     must already exist in these modes.
        /// </exception>
        /// <exception cref="T:System.IO.IOException">
        ///     An I/O error, such as specifying FileMode.CreateNew when the file specified
        ///     by <paramref name="path" /> already exists, occurred.-or-The stream has been closed.
        /// </exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">
        ///     The specified path is invalid, such as being on an unmapped
        ///     drive.
        /// </exception>
        /// <exception cref="T:System.IO.PathTooLongException">
        ///     The specified path, file name, or both exceed the system-defined
        ///     maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names
        ///     must be less than 260 characters.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="mode" /> contains an invalid value.
        /// </exception>
        public MyFileStream(string path, FileMode mode) : base(path, mode)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class with the specified path,
        ///     creation mode, and read/write permission.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate. </param>
        /// <param name="mode">A constant that determines how to open or create the file. </param>
        /// <param name="access">
        ///     A constant that determines how the file can be accessed by the FileStream object. This also
        ///     determines the values returned by the <see cref="P:System.IO.FileStream.CanRead" /> and
        ///     <see cref="P:System.IO.FileStream.CanWrite" /> properties of the FileStream object.
        ///     <see cref="P:System.IO.FileStream.CanSeek" /> is true if <paramref name="path" /> specifies a disk file.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="path" /> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="path" /> is an empty string (""), contains only white space, or contains one or more invalid
        ///     characters. -or-<paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an
        ///     NTFS environment.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///     <paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS
        ///     environment.
        /// </exception>
        /// <exception cref="T:System.IO.FileNotFoundException">
        ///     The file cannot be found, such as when <paramref name="mode" /> is
        ///     FileMode.Truncate or FileMode.Open, and the file specified by <paramref name="path" /> does not exist. The file
        ///     must already exist in these modes.
        /// </exception>
        /// <exception cref="T:System.IO.IOException">
        ///     An I/O error, such as specifying FileMode.CreateNew when the file specified
        ///     by <paramref name="path" /> already exists, occurred. -or-The stream has been closed.
        /// </exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">
        ///     The specified path is invalid, such as being on an unmapped
        ///     drive.
        /// </exception>
        /// <exception cref="T:System.UnauthorizedAccessException">
        ///     The <paramref name="access" /> requested is not permitted by the
        ///     operating system for the specified <paramref name="path" />, such as when <paramref name="access" /> is Write or
        ///     ReadWrite and the file or directory is set for read-only access.
        /// </exception>
        /// <exception cref="T:System.IO.PathTooLongException">
        ///     The specified path, file name, or both exceed the system-defined
        ///     maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names
        ///     must be less than 260 characters.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="mode" /> contains an invalid value.
        /// </exception>
        public MyFileStream(string path, FileMode mode, FileAccess access) : base(path, mode, access)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class with the specified path,
        ///     creation mode, read/write permission, and sharing permission.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate. </param>
        /// <param name="mode">A constant that determines how to open or create the file. </param>
        /// <param name="access">
        ///     A constant that determines how the file can be accessed by the FileStream object. This also
        ///     determines the values returned by the <see cref="P:System.IO.FileStream.CanRead" /> and
        ///     <see cref="P:System.IO.FileStream.CanWrite" /> properties of the FileStream object.
        ///     <see cref="P:System.IO.FileStream.CanSeek" /> is true if <paramref name="path" /> specifies a disk file.
        /// </param>
        /// <param name="share">A constant that determines how the file will be shared by processes. </param>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="path" /> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="path" /> is an empty string (""), contains only white space, or contains one or more invalid
        ///     characters. -or-<paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an
        ///     NTFS environment.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///     <paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS
        ///     environment.
        /// </exception>
        /// <exception cref="T:System.IO.FileNotFoundException">
        ///     The file cannot be found, such as when <paramref name="mode" /> is
        ///     FileMode.Truncate or FileMode.Open, and the file specified by <paramref name="path" /> does not exist. The file
        ///     must already exist in these modes.
        /// </exception>
        /// <exception cref="T:System.IO.IOException">
        ///     An I/O error, such as specifying FileMode.CreateNew when the file specified
        ///     by <paramref name="path" /> already exists, occurred. -or-The system is running Windows 98 or Windows 98 Second
        ///     Edition and <paramref name="share" /> is set to FileShare.Delete.-or-The stream has been closed.
        /// </exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">
        ///     The specified path is invalid, such as being on an unmapped
        ///     drive.
        /// </exception>
        /// <exception cref="T:System.UnauthorizedAccessException">
        ///     The <paramref name="access" /> requested is not permitted by the
        ///     operating system for the specified <paramref name="path" />, such as when <paramref name="access" /> is Write or
        ///     ReadWrite and the file or directory is set for read-only access.
        /// </exception>
        /// <exception cref="T:System.IO.PathTooLongException">
        ///     The specified path, file name, or both exceed the system-defined
        ///     maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names
        ///     must be less than 260 characters.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="mode" /> contains an invalid value.
        /// </exception>
        public MyFileStream(string path, FileMode mode, FileAccess access, FileShare share) : base(path, mode, access, share)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class with the specified path,
        ///     creation mode, read/write and sharing permission, and buffer size.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate. </param>
        /// <param name="mode">A constant that determines how to open or create the file. </param>
        /// <param name="access">
        ///     A constant that determines how the file can be accessed by the FileStream object. This also
        ///     determines the values returned by the <see cref="P:System.IO.FileStream.CanRead" /> and
        ///     <see cref="P:System.IO.FileStream.CanWrite" /> properties of the FileStream object.
        ///     <see cref="P:System.IO.FileStream.CanSeek" /> is true if <paramref name="path" /> specifies a disk file.
        /// </param>
        /// <param name="share">A constant that determines how the file will be shared by processes. </param>
        /// <param name="bufferSize">
        ///     A positive <see cref="T:System.Int32" /> value greater than 0 indicating the buffer size. The
        ///     default buffer size is 4096.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="path" /> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="path" /> is an empty string (""), contains only white space, or contains one or more invalid
        ///     characters. -or-<paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an
        ///     NTFS environment.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///     <paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS
        ///     environment.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="bufferSize" /> is negative or zero.-or- <paramref name="mode" />, <paramref name="access" />, or
        ///     <paramref name="share" /> contain an invalid value.
        /// </exception>
        /// <exception cref="T:System.IO.FileNotFoundException">
        ///     The file cannot be found, such as when <paramref name="mode" /> is
        ///     FileMode.Truncate or FileMode.Open, and the file specified by <paramref name="path" /> does not exist. The file
        ///     must already exist in these modes.
        /// </exception>
        /// <exception cref="T:System.IO.IOException">
        ///     An I/O error, such as specifying FileMode.CreateNew when the file specified
        ///     by <paramref name="path" /> already exists, occurred. -or-The system is running Windows 98 or Windows 98 Second
        ///     Edition and <paramref name="share" /> is set to FileShare.Delete.-or-The stream has been closed.
        /// </exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">
        ///     The specified path is invalid, such as being on an unmapped
        ///     drive.
        /// </exception>
        /// <exception cref="T:System.UnauthorizedAccessException">
        ///     The <paramref name="access" /> requested is not permitted by the
        ///     operating system for the specified <paramref name="path" />, such as when <paramref name="access" /> is Write or
        ///     ReadWrite and the file or directory is set for read-only access.
        /// </exception>
        /// <exception cref="T:System.IO.PathTooLongException">
        ///     The specified path, file name, or both exceed the system-defined
        ///     maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names
        ///     must be less than 260 characters.
        /// </exception>
        public MyFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize) : base(path, mode, access, share, bufferSize)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class with the specified path,
        ///     creation mode, read/write and sharing permission, the access other FileStreams can have to the same file, the
        ///     buffer size, and additional file options.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate. </param>
        /// <param name="mode">A constant that determines how to open or create the file. </param>
        /// <param name="access">
        ///     A constant that determines how the file can be accessed by the FileStream object. This also
        ///     determines the values returned by the <see cref="P:System.IO.FileStream.CanRead" /> and
        ///     <see cref="P:System.IO.FileStream.CanWrite" /> properties of the FileStream object.
        ///     <see cref="P:System.IO.FileStream.CanSeek" /> is true if <paramref name="path" /> specifies a disk file.
        /// </param>
        /// <param name="share">A constant that determines how the file will be shared by processes. </param>
        /// <param name="bufferSize">
        ///     A positive <see cref="T:System.Int32" /> value greater than 0 indicating the buffer size. The
        ///     default buffer size is 4096.
        /// </param>
        /// <param name="options">A value that specifies additional file options.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="path" /> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="path" /> is an empty string (""), contains only white space, or contains one or more invalid
        ///     characters. -or-<paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an
        ///     NTFS environment.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///     <paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS
        ///     environment.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="bufferSize" /> is negative or zero.-or- <paramref name="mode" />, <paramref name="access" />, or
        ///     <paramref name="share" /> contain an invalid value.
        /// </exception>
        /// <exception cref="T:System.IO.FileNotFoundException">
        ///     The file cannot be found, such as when <paramref name="mode" /> is
        ///     FileMode.Truncate or FileMode.Open, and the file specified by <paramref name="path" /> does not exist. The file
        ///     must already exist in these modes.
        /// </exception>
        /// <exception cref="T:System.IO.IOException">
        ///     An I/O error, such as specifying FileMode.CreateNew when the file specified
        ///     by <paramref name="path" /> already exists, occurred.-or-The stream has been closed.
        /// </exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">
        ///     The specified path is invalid, such as being on an unmapped
        ///     drive.
        /// </exception>
        /// <exception cref="T:System.UnauthorizedAccessException">
        ///     The <paramref name="access" /> requested is not permitted by the
        ///     operating system for the specified <paramref name="path" />, such as when <paramref name="access" /> is Write or
        ///     ReadWrite and the file or directory is set for read-only access. -or-
        ///     <see cref="F:System.IO.FileOptions.Encrypted" /> is specified for <paramref name="options" />, but file encryption
        ///     is not supported on the current platform.
        /// </exception>
        /// <exception cref="T:System.IO.PathTooLongException">
        ///     The specified path, file name, or both exceed the system-defined
        ///     maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names
        ///     must be less than 260 characters.
        /// </exception>
        public MyFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options) : base(path, mode, access, share, bufferSize, options)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class with the specified path,
        ///     creation mode, read/write and sharing permission, buffer size, and synchronous or asynchronous state.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate. </param>
        /// <param name="mode">A constant that determines how to open or create the file. </param>
        /// <param name="access">
        ///     A constant that determines how the file can be accessed by the FileStream object. This also
        ///     determines the values returned by the <see cref="P:System.IO.FileStream.CanRead" /> and
        ///     <see cref="P:System.IO.FileStream.CanWrite" /> properties of the FileStream object.
        ///     <see cref="P:System.IO.FileStream.CanSeek" /> is true if <paramref name="path" /> specifies a disk file.
        /// </param>
        /// <param name="share">A constant that determines how the file will be shared by processes. </param>
        /// <param name="bufferSize">
        ///     A positive <see cref="T:System.Int32" /> value greater than 0 indicating the buffer size. The
        ///     default buffer size is 4096..
        /// </param>
        /// <param name="useAsync">
        ///     Specifies whether to use asynchronous I/O or synchronous I/O. However, note that the underlying operating system
        ///     might not support asynchronous I/O, so when specifying true, the handle might be opened synchronously depending on
        ///     the platform. When opened asynchronously, the
        ///     <see
        ///         cref="M:System.IO.FileStream.BeginRead(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" />
        ///     and
        ///     <see
        ///         cref="M:System.IO.FileStream.BeginWrite(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" />
        ///     methods perform better on large reads or writes, but they might be much slower for small reads or writes. If the
        ///     application is designed to take advantage of asynchronous I/O, set the <paramref name="useAsync" /> parameter to
        ///     true. Using asynchronous I/O correctly can speed up applications by as much as a factor of 10, but using it without
        ///     redesigning the application for asynchronous I/O can decrease performance by as much as a factor of 10.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="path" /> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="path" /> is an empty string (""), contains only white space, or contains one or more invalid
        ///     characters. -or-<paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an
        ///     NTFS environment.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///     <paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS
        ///     environment.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="bufferSize" /> is negative or zero.-or- <paramref name="mode" />, <paramref name="access" />, or
        ///     <paramref name="share" /> contain an invalid value.
        /// </exception>
        /// <exception cref="T:System.IO.FileNotFoundException">
        ///     The file cannot be found, such as when <paramref name="mode" /> is
        ///     FileMode.Truncate or FileMode.Open, and the file specified by <paramref name="path" /> does not exist. The file
        ///     must already exist in these modes.
        /// </exception>
        /// <exception cref="T:System.IO.IOException">
        ///     An I/O error, such as specifying FileMode.CreateNew when the file specified
        ///     by <paramref name="path" /> already exists, occurred.-or- The system is running Windows 98 or Windows 98 Second
        ///     Edition and <paramref name="share" /> is set to FileShare.Delete.-or-The stream has been closed.
        /// </exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">
        ///     The specified path is invalid, such as being on an unmapped
        ///     drive.
        /// </exception>
        /// <exception cref="T:System.UnauthorizedAccessException">
        ///     The <paramref name="access" /> requested is not permitted by the
        ///     operating system for the specified <paramref name="path" />, such as when <paramref name="access" /> is Write or
        ///     ReadWrite and the file or directory is set for read-only access.
        /// </exception>
        /// <exception cref="T:System.IO.PathTooLongException">
        ///     The specified path, file name, or both exceed the system-defined
        ///     maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names
        ///     must be less than 260 characters.
        /// </exception>
        public MyFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync) : base(path, mode, access, share, bufferSize, useAsync)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class with the specified path,
        ///     creation mode, access rights and sharing permission, the buffer size, additional file options, access control and
        ///     audit security.
        /// </summary>
        /// <param name="path">
        ///     A relative or absolute path for the file that the current <see cref="T:System.IO.FileStream" />
        ///     object will encapsulate.
        /// </param>
        /// <param name="mode">A constant that determines how to open or create the file.</param>
        /// <param name="rights">
        ///     A constant that determines the access rights to use when creating access and audit rules for the
        ///     file.
        /// </param>
        /// <param name="share">A constant that determines how the file will be shared by processes.</param>
        /// <param name="bufferSize">
        ///     A positive <see cref="T:System.Int32" /> value greater than 0 indicating the buffer size. The
        ///     default buffer size is 4096.
        /// </param>
        /// <param name="options">A constant that specifies additional file options.</param>
        /// <param name="fileSecurity">A constant that determines the access control and audit security for the file.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="path" /> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="path" /> is an empty string (""), contains only white space, or contains one or more invalid
        ///     characters. -or-<paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an
        ///     NTFS environment.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///     <paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS
        ///     environment.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="bufferSize" /> is negative or zero.-or- <paramref name="mode" />, <paramref name="access" />, or
        ///     <paramref name="share" /> contain an invalid value.
        /// </exception>
        /// <exception cref="T:System.IO.FileNotFoundException">
        ///     The file cannot be found, such as when <paramref name="mode" /> is
        ///     FileMode.Truncate or FileMode.Open, and the file specified by <paramref name="path" /> does not exist. The file
        ///     must already exist in these modes.
        /// </exception>
        /// <exception cref="T:System.IO.IOException">
        ///     An I/O error, such as specifying FileMode.CreateNew when the file specified
        ///     by <paramref name="path" /> already exists, occurred. -or-The stream has been closed.
        /// </exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">
        ///     The specified path is invalid, such as being on an unmapped
        ///     drive.
        /// </exception>
        /// <exception cref="T:System.UnauthorizedAccessException">
        ///     The <paramref name="access" /> requested is not permitted by the
        ///     operating system for the specified <paramref name="path" />, such as when <paramref name="access" /> is Write or
        ///     ReadWrite and the file or directory is set for read-only access. -or-
        ///     <see cref="F:System.IO.FileOptions.Encrypted" /> is specified for <paramref name="options" />, but file encryption
        ///     is not supported on the current platform.
        /// </exception>
        /// <exception cref="T:System.IO.PathTooLongException">
        ///     The specified <paramref name="path" />, file name, or both exceed
        ///     the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters,
        ///     and file names must be less than 260 characters.
        /// </exception>
        /// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
        public MyFileStream(string path, FileMode mode, FileSystemRights rights, FileShare share, int bufferSize, FileOptions options, FileSecurity fileSecurity)
            : base(path, mode, rights, share, bufferSize, options, fileSecurity)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class with the specified path,
        ///     creation mode, access rights and sharing permission, the buffer size, and additional file options.
        /// </summary>
        /// <param name="path">
        ///     A relative or absolute path for the file that the current <see cref="T:System.IO.FileStream" />
        ///     object will encapsulate.
        /// </param>
        /// <param name="mode">A constant that determines how to open or create the file.</param>
        /// <param name="rights">
        ///     A constant that determines the access rights to use when creating access and audit rules for the
        ///     file.
        /// </param>
        /// <param name="share">A constant that determines how the file will be shared by processes.</param>
        /// <param name="bufferSize">
        ///     A positive <see cref="T:System.Int32" /> value greater than 0 indicating the buffer size. The
        ///     default buffer size is 4096.
        /// </param>
        /// <param name="options">A constant that specifies additional file options.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="path" /> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="path" /> is an empty string (""), contains only white space, or contains one or more invalid
        ///     characters. -or-<paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an
        ///     NTFS environment.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///     <paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS
        ///     environment.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="bufferSize" /> is negative or zero.-or- <paramref name="mode" />, <paramref name="access" />, or
        ///     <paramref name="share" /> contain an invalid value.
        /// </exception>
        /// <exception cref="T:System.IO.FileNotFoundException">
        ///     The file cannot be found, such as when <paramref name="mode" /> is
        ///     FileMode.Truncate or FileMode.Open, and the file specified by <paramref name="path" /> does not exist. The file
        ///     must already exist in these modes.
        /// </exception>
        /// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
        /// <exception cref="T:System.IO.IOException">
        ///     An I/O error, such as specifying FileMode.CreateNew when the file specified
        ///     by <paramref name="path" /> already exists, occurred. -or-The stream has been closed.
        /// </exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">
        ///     The specified path is invalid, such as being on an unmapped
        ///     drive.
        /// </exception>
        /// <exception cref="T:System.UnauthorizedAccessException">
        ///     The <paramref name="access" /> requested is not permitted by the
        ///     operating system for the specified <paramref name="path" />, such as when <paramref name="access" /> is Write or
        ///     ReadWrite and the file or directory is set for read-only access. -or-
        ///     <see cref="F:System.IO.FileOptions.Encrypted" /> is specified for <paramref name="options" />, but file encryption
        ///     is not supported on the current platform.
        /// </exception>
        /// <exception cref="T:System.IO.PathTooLongException">
        ///     The specified <paramref name="path" />, file name, or both exceed
        ///     the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters,
        ///     and file names must be less than 260 characters.
        /// </exception>
        public MyFileStream(string path, FileMode mode, FileSystemRights rights, FileShare share, int bufferSize, FileOptions options) : base(path, mode, rights, share, bufferSize, options)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class for the specified file handle,
        ///     with the specified read/write permission.
        /// </summary>
        /// <param name="handle">A file handle for the file that the current FileStream object will encapsulate. </param>
        /// <param name="access">
        ///     A constant that sets the <see cref="P:System.IO.FileStream.CanRead" /> and
        ///     <see cref="P:System.IO.FileStream.CanWrite" /> properties of the FileStream object.
        /// </param>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="access" /> is not a field of <see cref="T:System.IO.FileAccess" />.
        /// </exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error, such as a disk error, occurred.-or-The stream has been closed. </exception>
        /// <exception cref="T:System.UnauthorizedAccessException">
        ///     The <paramref name="access" /> requested is not permitted by the
        ///     operating system for the specified file handle, such as when <paramref name="access" /> is Write or ReadWrite and
        ///     the file handle is set for read-only access.
        /// </exception>
        public MyFileStream(IntPtr handle, FileAccess access) : base(handle, access)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class for the specified file handle,
        ///     with the specified read/write permission and FileStream instance ownership.
        /// </summary>
        /// <param name="handle">A file handle for the file that the current FileStream object will encapsulate. </param>
        /// <param name="access">
        ///     A constant that sets the <see cref="P:System.IO.FileStream.CanRead" /> and
        ///     <see cref="P:System.IO.FileStream.CanWrite" /> properties of the FileStream object.
        /// </param>
        /// <param name="ownsHandle">true if the file handle will be owned by this FileStream instance; otherwise, false. </param>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="access" /> is not a field of <see cref="T:System.IO.FileAccess" />.
        /// </exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error, such as a disk error, occurred.-or-The stream has been closed. </exception>
        /// <exception cref="T:System.UnauthorizedAccessException">
        ///     The <paramref name="access" /> requested is not permitted by the
        ///     operating system for the specified file handle, such as when <paramref name="access" /> is Write or ReadWrite and
        ///     the file handle is set for read-only access.
        /// </exception>
        public MyFileStream(IntPtr handle, FileAccess access, bool ownsHandle) : base(handle, access, ownsHandle)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class for the specified file handle,
        ///     with the specified read/write permission, FileStream instance ownership, and buffer size.
        /// </summary>
        /// <param name="handle">A file handle for the file that this FileStream object will encapsulate. </param>
        /// <param name="access">
        ///     A constant that sets the <see cref="P:System.IO.FileStream.CanRead" /> and
        ///     <see cref="P:System.IO.FileStream.CanWrite" /> properties of the FileStream object.
        /// </param>
        /// <param name="ownsHandle">true if the file handle will be owned by this FileStream instance; otherwise, false. </param>
        /// <param name="bufferSize">
        ///     A positive <see cref="T:System.Int32" /> value greater than 0 indicating the buffer size. The
        ///     default buffer size is 4096.
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="bufferSize" /> is negative.
        /// </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error, such as a disk error, occurred.-or-The stream has been closed. </exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        /// <exception cref="T:System.UnauthorizedAccessException">
        ///     The <paramref name="access" /> requested is not permitted by the
        ///     operating system for the specified file handle, such as when <paramref name="access" /> is Write or ReadWrite and
        ///     the file handle is set for read-only access.
        /// </exception>
        public MyFileStream(IntPtr handle, FileAccess access, bool ownsHandle, int bufferSize) : base(handle, access, ownsHandle, bufferSize)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class for the specified file handle,
        ///     with the specified read/write permission, FileStream instance ownership, buffer size, and synchronous or
        ///     asynchronous state.
        /// </summary>
        /// <param name="handle">A file handle for the file that this FileStream object will encapsulate. </param>
        /// <param name="access">
        ///     A constant that sets the <see cref="P:System.IO.FileStream.CanRead" /> and
        ///     <see cref="P:System.IO.FileStream.CanWrite" /> properties of the FileStream object.
        /// </param>
        /// <param name="ownsHandle">true if the file handle will be owned by this FileStream instance; otherwise, false. </param>
        /// <param name="bufferSize">
        ///     A positive <see cref="T:System.Int32" /> value greater than 0 indicating the buffer size. The
        ///     default buffer size is 4096.
        /// </param>
        /// <param name="isAsync">true if the handle was opened asynchronously (that is, in overlapped I/O mode); otherwise, false. </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="access" /> is less than FileAccess.Read or greater than FileAccess.ReadWrite or
        ///     <paramref name="bufferSize" /> is less than or equal to 0.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">The handle is invalid. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error, such as a disk error, occurred.-or-The stream has been closed. </exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        /// <exception cref="T:System.UnauthorizedAccessException">
        ///     The <paramref name="access" /> requested is not permitted by the
        ///     operating system for the specified file handle, such as when <paramref name="access" /> is Write or ReadWrite and
        ///     the file handle is set for read-only access.
        /// </exception>
        public MyFileStream(IntPtr handle, FileAccess access, bool ownsHandle, int bufferSize, bool isAsync) : base(handle, access, ownsHandle, bufferSize, isAsync)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class for the specified file handle,
        ///     with the specified read/write permission.
        /// </summary>
        /// <param name="handle">A file handle for the file that the current FileStream object will encapsulate. </param>
        /// <param name="access">
        ///     A constant that sets the <see cref="P:System.IO.FileStream.CanRead" /> and
        ///     <see cref="P:System.IO.FileStream.CanWrite" /> properties of the FileStream object.
        /// </param>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="access" /> is not a field of <see cref="T:System.IO.FileAccess" />.
        /// </exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error, such as a disk error, occurred.-or-The stream has been closed. </exception>
        /// <exception cref="T:System.UnauthorizedAccessException">
        ///     The <paramref name="access" /> requested is not permitted by the
        ///     operating system for the specified file handle, such as when <paramref name="access" /> is Write or ReadWrite and
        ///     the file handle is set for read-only access.
        /// </exception>
        public MyFileStream([NotNull] SafeFileHandle handle, FileAccess access) : base(handle, access)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class for the specified file handle,
        ///     with the specified read/write permission, and buffer size.
        /// </summary>
        /// <param name="handle">A file handle for the file that the current FileStream object will encapsulate. </param>
        /// <param name="access">
        ///     A <see cref="T:System.IO.FileAccess" /> constant that sets the
        ///     <see cref="P:System.IO.FileStream.CanRead" /> and <see cref="P:System.IO.FileStream.CanWrite" /> properties of the
        ///     FileStream object.
        /// </param>
        /// <param name="bufferSize">
        ///     A positive <see cref="T:System.Int32" /> value greater than 0 indicating the buffer size. The
        ///     default buffer size is 4096.
        /// </param>
        /// <exception cref="T:System.ArgumentException">
        ///     The <paramref name="handle" /> parameter is an invalid handle.-or-The
        ///     <paramref name="handle" /> parameter is a synchronous handle and it was used asynchronously.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="bufferSize" /> parameter is negative. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error, such as a disk error, occurred.-or-The stream has been closed.  </exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        /// <exception cref="T:System.UnauthorizedAccessException">
        ///     The <paramref name="access" /> requested is not permitted by the
        ///     operating system for the specified file handle, such as when <paramref name="access" /> is Write or ReadWrite and
        ///     the file handle is set for read-only access.
        /// </exception>
        public MyFileStream([NotNull] SafeFileHandle handle, FileAccess access, int bufferSize) : base(handle, access, bufferSize)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class for the specified file handle,
        ///     with the specified read/write permission, buffer size, and synchronous or asynchronous state.
        /// </summary>
        /// <param name="handle">A file handle for the file that this FileStream object will encapsulate. </param>
        /// <param name="access">
        ///     A constant that sets the <see cref="P:System.IO.FileStream.CanRead" /> and
        ///     <see cref="P:System.IO.FileStream.CanWrite" /> properties of the FileStream object.
        /// </param>
        /// <param name="bufferSize">
        ///     A positive <see cref="T:System.Int32" /> value greater than 0 indicating the buffer size. The
        ///     default buffer size is 4096.
        /// </param>
        /// <param name="isAsync">true if the handle was opened asynchronously (that is, in overlapped I/O mode); otherwise, false. </param>
        /// <exception cref="T:System.ArgumentException">
        ///     The <paramref name="handle" /> parameter is an invalid handle.-or-The
        ///     <paramref name="handle" /> parameter is a synchronous handle and it was used asynchronously.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="bufferSize" /> parameter is negative. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error, such as a disk error, occurred.-or-The stream has been closed.  </exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        /// <exception cref="T:System.UnauthorizedAccessException">
        ///     The <paramref name="access" /> requested is not permitted by the
        ///     operating system for the specified file handle, such as when <paramref name="access" /> is Write or ReadWrite and
        ///     the file handle is set for read-only access.
        /// </exception>
        public MyFileStream([NotNull] SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync) : base(handle, access, bufferSize, isAsync)
        {
        }

        /// <summary>
        ///     Releases the unmanaged resources used by the <see cref="T:System.IO.FileStream" /> and optionally releases the
        ///     managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            // TODO test to ensure output stream is disposed.
            Debugger.Break();
            base.Dispose(disposing);
        }
    }
}