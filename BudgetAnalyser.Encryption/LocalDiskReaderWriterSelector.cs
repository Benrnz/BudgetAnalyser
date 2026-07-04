using System.Diagnostics.CodeAnalysis;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetAnalyser.Encryption;

[AutoRegisterWithIoC(SingleInstance = true)]
internal class LocalDiskReaderWriterSelector(
    [FromKeyedServices(StorageConstants.EncryptedInstanceName)]
    IFileReaderWriter encrypted,
    [FromKeyedServices(StorageConstants.UnprotectedInstanceName)]
    IFileReaderWriter unprotected)
    : IReaderWriterSelector
{
    private readonly IFileReaderWriter encryptedReaderWriter = encrypted ?? throw new ArgumentNullException(nameof(encrypted));
    private readonly IFileReaderWriter unprotectedReaderWriter = unprotected ?? throw new ArgumentNullException(nameof(unprotected));

    /// <summary>
    ///     Selects a repository implementation based on input parameters.
    /// </summary>
    /// <param name="isEncrypted">if set to <c>true</c> the storage files are encrypted.</param>
    /// <returns>An instance of the repository ready to use.</returns>
    [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "IFileReaderWriter")]
    public IFileReaderWriter SelectReaderWriter(bool isEncrypted)
    {
        if (this.unprotectedReaderWriter is null && this.encryptedReaderWriter is null)
        {
            throw new InvalidOperationException("Code Bug: There are no instances of IFileReaderWriter registered.");
        }

        var readerWriter = isEncrypted ? this.encryptedReaderWriter : this.unprotectedReaderWriter;

        return readerWriter ?? throw new InvalidOperationException("Code Bug: There are no instances of IFileReaderWriter registered for the selected encryption mode. " + isEncrypted);
    }
}
