using BudgetAnalyser.Engine.BankAccount;

namespace BudgetAnalyser.Engine.Statement;

/// <summary>
///     A <see cref="IBankStatementImporter" /> repository based on a list of dependency injected importers.
///     These are the bank specific file formats exported from online banking to be imported into the Budget Analyser.
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
public class BankStatementImporterRepository : IBankStatementImporterRepository
{
    private readonly IEnumerable<IBankStatementImporter> importers;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BankStatementImporterRepository" /> class.
    /// </summary>
    /// <param name="importers">The importers.</param>
    /// <exception cref="System.ArgumentNullException"></exception>
    public BankStatementImporterRepository(IEnumerable<IBankStatementImporter> importers)
    {
        // ReSharper disable PossibleMultipleEnumeration
        if (importers is null)
        {
            throw new ArgumentNullException(nameof(importers));
        }

        this.importers = importers.Any() ? importers.ToList() : throw new ArgumentException("There are no importers to import data, code error.");
    }

    /// <summary>
    ///     Import the given file.
    ///     If the file cannot be imported by any of these importers a <see cref="NotSupportedException" /> will be thrown.
    /// </summary>
    public async Task<StatementModel> ImportAsync(string fullFileName, Account account)
    {
        foreach (var importer in this.importers)
        {
            if (await importer.TasteTestAsync(fullFileName))
            {
                return await importer.LoadAsync(fullFileName, account);
            }
        }

        throw new NotSupportedException("The requested file name cannot be loaded. It is not of any known format.");
    }
}
