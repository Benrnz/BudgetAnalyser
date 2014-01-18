using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Account;

namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    /// A <see cref="IBankStatementImporter"/> repository based on a list of dependency injected importers.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class BankStatementImporterRepository : IBankStatementImporterRepository
    {
        private readonly IEnumerable<IBankStatementImporter> importers;

        public BankStatementImporterRepository(IEnumerable<IBankStatementImporter> importers)
        {
            this.importers = importers.ToList();
        }

        /// <summary>
        /// Can any importer in this repository read and import the given file.
        /// Calling this method will open the file and read some of its contents.
        /// </summary>
        /// <returns>True if this file can be imported one of the importers in the repository.</returns>
        public bool CanImport(string fullFileName)
        {
            return this.importers.Any(importer => importer.TasteTest(fullFileName));
        }

        /// <summary>
        /// Import the given file.  It is recommended to call <see cref="IBankStatementImporterRepository.CanImport"/> first.  If the file cannot
        /// be imported by any of this repositories importers a <see cref="NotSupportedException"/> will be thrown.
        /// </summary>
        public StatementModel Import(string fullFileName, AccountType accountType)
        {
            foreach (IBankStatementImporter importer in this.importers)
            {
                if (importer.TasteTest(fullFileName))
                {
                    return importer.Load(fullFileName, accountType);
                }
            }

            throw new NotSupportedException("The requested file name cannot be loaded. It is not of any known format.");
        }
    }
}