﻿using BudgetAnalyser.Engine.BankAccount;

namespace BudgetAnalyser.Engine.Statement;

/// <summary>
///     An individual bank statement export file importer.
///     One of these will be implemented for each export file format a bank uses.
///     For example ANZ exports a different file format for Credit Card accounts and Chequing and Saving Accounts. In this
///     case two implementations
///     of this interface is required.
/// </summary>
public interface IBankStatementImporter
{
    /// <summary>
    ///     Load the given file into a <see cref="StatementModel" />.
    /// </summary>
    /// <param name="fileName">The file to load.</param>
    /// <param name="account">
    ///     The account to classify these transactions. This is useful when merging one statement to another. For example,
    ///     merging a cheque account
    ///     export with visa account export, each can be classified using an account.
    /// </param>
    Task<StatementModel> LoadAsync(string fileName, Account account);

    /// <summary>
    ///     Test the given file to see if this importer implementation can read and import it.
    ///     This will open and read some of the contents of the file.
    /// </summary>
    Task<bool> TasteTestAsync(string fileName);
}
