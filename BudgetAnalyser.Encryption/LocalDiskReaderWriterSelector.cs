using System.Diagnostics.CodeAnalysis;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Encryption;

[AutoRegisterWithIoC(SingleInstance = true)]
internal class LocalDiskReaderWriterSelector : IReaderWriterSelector
{
    private readonly IFileReaderWriter encryptedReaderWriter;
    private readonly IFileReaderWriter unprotectedRaderWriter;

    public LocalDiskReaderWriterSelector(IEnumerable<IFileReaderWriter> allReaderWriters)
    {
        if (allReaderWriters is null)
        {
            throw new ArgumentNullException(nameof(allReaderWriters));
        }

        var fileReaderWriters = allReaderWriters.ToList();
        this.encryptedReaderWriter = DefaultIoCRegistrations.GetNamedInstance(fileReaderWriters, StorageConstants.EncryptedInstanceName);
        this.unprotectedRaderWriter = DefaultIoCRegistrations.GetNamedInstance(fileReaderWriters, StorageConstants.UnprotectedInstanceName);
    }

    /// <summary>
    ///     Selects a repository implementation based on input parameters.
    /// </summary>
    /// <param name="isEncrypted">if set to <c>true</c> the storage files are encrypted.</param>
    /// <returns>An instance of the repository ready to use.</returns>
    [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "IFileReaderWriter")]
    public IFileReaderWriter SelectReaderWriter(bool isEncrypted)
    {
        if (this.unprotectedRaderWriter is null && this.encryptedReaderWriter is null)
        {
            throw new InvalidOperationException("Code Bug: There are no instances of IFileReaderWriter registered.");
        }

        var readerWriter = isEncrypted ? this.encryptedReaderWriter : this.unprotectedRaderWriter;

        return readerWriter ?? throw new InvalidOperationException("Code Bug: There are no instances of IFileReaderWriter registered for the selected encryption mode. " + isEncrypted);
    }
}
