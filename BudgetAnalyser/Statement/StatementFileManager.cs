using System;
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
    ///     To function it orchestrates across the  <see cref="IStatementModelRepository" /> and the
    ///     <see cref="IBankStatementImporterRepository" />.
    ///     This implementation is strictly not thread safe and should be single threaded only.  Don't allow mulitple threads
    ///     to use it at the same time.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    internal class StatementFileManager : IStatementFileManager
    {
        private readonly IBankStatementImporterRepository importerRepository;
        private readonly LoadFileController loadFileController;
        private readonly IUserMessageBox messageBox;
        private readonly IStatementModelRepository statementModelRepository;
        private Func<IWaitCursor> waitCursorFactory;

        public StatementFileManager(
            [NotNull] LoadFileController loadFileController,
            [NotNull] IStatementModelRepository statementModelRepository,
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
            this.waitCursorFactory = uiContext.WaitCursorFactory;
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
        public StatementModel ImportAndMergeBankStatement(StatementModel statementModel, bool throwIfFileNotFound = false)
        {
            string fileName = GetFileNameFromUser(OpenMode.Merge, statementModel);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                // User cancelled
                return null;
            }

            return LoadAnyStatementFile(fileName);
        }

        /// <summary>
        ///     Loads either an existing Budget Analyser Statement file, or creates a new Budget Analyser Statement file from a
        ///     known bank statement
        ///     file.
        /// </summary>
        /// <param name="fileName">Pass a known filename to load or null to prompt the user to choose a file.</param>
        public StatementModel LoadAnyStatementFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = GetFileNameFromUser(OpenMode.Open);
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    // User cancelled
                    return null;
                }
            }

            return LoadStatement(fileName);
        }

        /// <summary>
        ///     Save the given statement model into a file. The file will be created or updated and will be a Budget Analyser
        ///     Statement file format.
        ///     Saving and preserving bank statement files is not supported.
        /// </summary>
        public void Save(StatementModel statementModel)
        {
            this.statementModelRepository.Save(statementModel, statementModel.FileName);
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
        private string GetFileNameFromUser(OpenMode mode, StatementModel statementModel = null)
        {
            switch (mode)
            {
                case OpenMode.Merge:
                    this.loadFileController.RequestUserInputForMerging(statementModel);
                    break;

                case OpenMode.Open:
                    this.loadFileController.RequestUserInputForOpenFile();
                    break;
            }

            return this.loadFileController.FileName;
        }

        private StatementModel LoadStatement(string fullFileName)
        {
            // TODO add generic UI to let user classify columns.
            IWaitCursor waitCursor = null;
            try
            {
                waitCursor = this.waitCursorFactory();
                switch (this.loadFileController.LastFileWasBudgetAnalyserStatementFile)
                {
                    case null:
                        // Load File Controller was not called because a filename was already known, so check to see if file is a valid analyser file.
                        if (this.statementModelRepository.IsValidFile(fullFileName))
                        {
                            return this.statementModelRepository.Load(fullFileName);
                        }
                        break;

                    case true:
                        // Load File Controller was called and the file chosen was found to be a Budget Analyser statement file.
                        return this.statementModelRepository.Load(fullFileName);
                }

                // Anything not found to be a budget analyser statement file is given to the importer repository to see if it has any importer that
                // can import the file.
                if (this.importerRepository.CanImport(fullFileName))
                {
                    return this.importerRepository.Import(fullFileName, this.loadFileController.AccountType);
                }

                throw new NotSupportedException("The specified file is not a valid Budget Analyser Statement file, or any known bank statement file.");
            }
            catch (FileFormatException ex)
            {
                this.messageBox.Show("The file cannot be loaded.\n" + ex.Message);
                return null;
            }
            catch (NotSupportedException ex)
            {
                this.messageBox.Show("The file cannot be loaded.\n" + ex.Message);
                return null;
            }
            catch (StatementModelCheckSumException ex)
            {
                this.messageBox.Show("The file being loaded is corrupt. The internal checksum does not match the transactions.\nFile Checksum:" + ex.FileChecksum);
                return null;
            }
            finally
            {
                this.loadFileController.Reset();
                if (waitCursor != null)
                {
                    waitCursor.Dispose();
                }
            }
        }
    }
}