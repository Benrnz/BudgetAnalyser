namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    ///     A repository for <see cref="StatementModel" />.
    /// </summary>
    public interface IStatementModelRepository
    {
        /// <summary>
        ///     Check to see if the repository contains and recognises the given file.
        /// </summary>
        bool IsValidFile(string fileName);

        /// <summary>
        ///     Load a <see cref="StatementModel" /> given the filename.
        /// </summary>
        StatementModel Load(string fileName);

        /// <summary>
        ///     Save a <see cref="StatementModel" /> so it can be recalled later.
        /// </summary>
        void Save(StatementModel model, string fileName);
    }
}