using System;
using System.Threading.Tasks;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Statement;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.Statement
{
    /// <summary>
    ///     The Statement File Manager is responsible for loading any kind of statement file, whether it be an existing budget
    ///     analyser statement file or a downloaded bank statement export to merge or load. If the filename is already known
    ///     it is loaded with no prompting, otherwise the user is prompted for a filename.
    ///     It also is responsible for saving  any open statement file into a budget analyser statement file.
    ///     To function it orchestrates across the  <see cref="IVersionedStatementModelRepository" /> and the
    ///     <see cref="IBankStatementImporterRepository" />.
    ///     This implementation is strictly not thread safe and should be single threaded only.  Don't allow mulitple threads
    ///     to use it at the same time.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification="Used by IoC")]
    [AutoRegisterWithIoC(SingleInstance = true)]
    internal class StatementFileManager : IStatementFileManager
    {
        private readonly IBankStatementImporterRepository importerRepository;
        private readonly LoadFileController loadFileController;
        private readonly IUserMessageBox messageBox;
        private readonly IVersionedStatementModelRepository statementModelRepository;

        public StatementFileManager(
            [NotNull] LoadFileController loadFileController,
            [NotNull] IVersionedStatementModelRepository statementModelRepository,
            [NotNull] IBankStatementImporterRepository importerRepository,
            [NotNull] UiContext uiContext)
        {
            if (loadFileController == null)
            {
                throw new ArgumentNullException("loadFileController");
            }

            if (statementModelRepository == null)
            {
                throw new ArgumentNullException("statementModelRepository");
            }

            if (importerRepository == null)
            {
                throw new ArgumentNullException("importerRepository");
            }

            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            this.loadFileController = loadFileController;
            this.statementModelRepository = statementModelRepository;
            this.importerRepository = importerRepository;
            this.messageBox = uiContext.UserPrompts.MessageBox;
        }

        private enum OpenMode
        {
            Open,
            Merge,
        }

        /// <summary>
        ///     Imports a bank statement file and merges it into an existing statement model. You cannot merge two Budget Analyser
        ///     Statement files.
        /// </summary>
        public async Task<StatementModel> ImportAndMergeBankStatementAsync(StatementModel statementModel, bool throwIfFileNotFound = false)
        {
            var fileName = await GetFileNameFromUser(OpenMode.Merge, statementModel);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                // User cancelled
                return null;
            }

            return await LoadAnyStatementFileAsync(fileName);
        }

        /// <summary>
        ///     Loads either an existing Budget Analyser Statement file, or creates a new Budget Analyser Statement file from a
        ///     known bank statement
        ///     file.
        /// </summary>
        /// <param name="fileName">Pass a known filename to load or null to prompt the user to choose a file.</param>
        public async Task<StatementModel> LoadAnyStatementFileAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = await GetFileNameFromUser(OpenMode.Open);
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    // User cancelled
                    return null;
                }
            }

            return await LoadStatementAsync(fileName);
        }

        /// <summary>
        ///     Save the given statement model into a file. The file will be created or updated and will be a Budget Analyser
        ///     Statement file format.
        ///     Saving and preserving bank statement files is not supported.
        /// </summary>
        public async Task SaveAsync([NotNull] StatementModel statementModel)
        {
            if (statementModel == null)
            {
                throw new ArgumentNullException("statementModel");
            }

            await this.statementModelRepository.SaveAsync(statementModel, statementModel.StorageKey);
        }

        /// <summary>
        ///     Prompts the user for a filename and other required parameters to be able to load/import/merge the file.
        /// </summary>
        /// <param name="mode">Open or Merge mode.</param>
        /// <param name="statementModel">
        ///     The currently loaded statement model. Only required for merging.  It is used to suggest a
        ///     date range only.
        /// </param>
        /// <returns>
        ///     The user selected filename. All other required parameters are accessible from the
        ///     <see cref="LoadFileController" />.
        /// </returns>
        private async Task<string> GetFileNameFromUser(OpenMode mode, StatementModel statementModel = null)
        {
            switch (mode)
            {
                case OpenMode.Merge:
                    await this.loadFileController.RequestUserInputForMerging(statementModel);
                    break;

                case OpenMode.Open:
                    await this.loadFileController.RequestUserInputForOpenFile();
                    break;
            }

            return this.loadFileController.FileName;
        }

        private async Task<StatementModel> LoadStatementAsync(string fullFileName)
        {
            // TODO add generic UI to let user classify columns.
            try
            {
                switch (this.loadFileController.LastFileWasBudgetAnalyserStatementFile)
                {
                    case null:
                        // Load File Controller was not called because a filename was already known, so check to see if file is a valid analyser file.
                        if (await this.statementModelRepository.IsValidFileAsync(fullFileName))
                        {
                            return await this.statementModelRepository.LoadAsync(fullFileName);
                        }
                        break;

                    case true:
                        // Load File Controller was called and the file chosen was found to be a Budget Analyser statement file.
                        return await this.statementModelRepository.LoadAsync(fullFileName);
                }

                // Anything not found to be a budget analyser statement file is given to the importer repository to see if it has any importer that
                // can import the file.
                if (this.importerRepository.CanImport(fullFileName))
                {
                    // TODO this should be async also
                    return this.importerRepository.Import(fullFileName, this.loadFileController.SelectedExistingAccountName);
                }

                throw new NotSupportedException("The specified file is not a valid Budget Analyser Statement file, or any known bank statement file.");
            }
            catch (FileFormatException ex)
            {
                FileCannotBeLoaded(ex);
                return null;
            }
            catch (NotSupportedException ex)
            {
                FileCannotBeLoaded(ex);
                return null;
            }
            catch (StatementModelChecksumException ex)
            {
                this.messageBox.Show("The file being loaded is corrupt. The internal checksum does not match the transactions.\nFile Checksum:" + ex.FileChecksum);
                return null;
            }
            finally
            {
                this.loadFileController.Reset();
            }
        }

        private void FileCannotBeLoaded(Exception ex)
        {
            this.messageBox.Show("The file cannot be loaded.\n" + ex.Message);
        }
    }
}