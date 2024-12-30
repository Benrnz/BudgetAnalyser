using System;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    ///     An exception to report that an attempt was made to import the same exact bank export twice.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class TransactionsAlreadyImportedException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionsAlreadyImportedException" /> class.
        /// </summary>
        public TransactionsAlreadyImportedException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionsAlreadyImportedException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public TransactionsAlreadyImportedException(string message) : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionsAlreadyImportedException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        [UsedImplicitly]
        public TransactionsAlreadyImportedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
