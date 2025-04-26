namespace BudgetAnalyser.Engine.Statement.Data;

internal record TransactionHeaderDto(long Checksum, string StorageKey, DateTime LastImport, string VersionHash)
{
    public TransactionHeaderDto(ReadOnlySpan<char> headerData, BankImportUtilities importUtilities, string storageKey) :
        this(
            importUtilities.FetchLong(headerData, 3),
            storageKey,
            importUtilities.FetchDateTime(headerData, 4),
            importUtilities.FetchString(headerData, 1))
    {
    }
}
