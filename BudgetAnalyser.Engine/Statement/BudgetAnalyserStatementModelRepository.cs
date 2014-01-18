using System;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    /// An implementation of <see cref="IStatementModelRepository"/> based on the local file system.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class BudgetAnalyserStatementModelRepository : IStatementModelRepository
    {
        // In future there may be different compatible versions of Budget Analyser Statement files that can be imported. For now there is only one.
        private readonly IVersionedStatementModelImporter analyserStatementFormatImporter;

        public BudgetAnalyserStatementModelRepository([NotNull] IVersionedStatementModelImporter analyserStatementFormatImporter)
        {
            if (analyserStatementFormatImporter == null)
            {
                throw new ArgumentNullException("analyserStatementFormatImporter");
            }

            this.analyserStatementFormatImporter = analyserStatementFormatImporter;
        }

        /// <summary>
        /// Check to see if the repository contains and recognises the given file.
        /// </summary>
        public bool IsValidFile([NotNull] string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            return this.analyserStatementFormatImporter.IsValidFile(fileName);
        }

        /// <summary>
        /// Load a <see cref="StatementModel"/> given the filename.
        /// </summary>
        public StatementModel Load([NotNull] string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            // In future there may be different compatible versions of Budget Analyser Statement files that can be imported. For now there is only one.
            return this.analyserStatementFormatImporter.Load(fileName);
        }

        /// <summary>
        /// Save a <see cref="StatementModel"/> so it can be recalled later.
        /// </summary>
        public void Save([NotNull] StatementModel model, [NotNull] string fileName)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            this.analyserStatementFormatImporter.Save(model, fileName);
        }
    }
}