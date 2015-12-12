using System;
using System.Runtime.Serialization;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    ///     An exception to report that an attempt was made to import the same exact bank export twice.
    /// </summary>
    [Serializable]
    public class TransactionsAlreadyImportedException : Exception
    {
        public TransactionsAlreadyImportedException()
        {
        }

        public TransactionsAlreadyImportedException(string message) : base(message)
        {
        }

        [UsedImplicitly]
        public TransactionsAlreadyImportedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnexpectedIndexException" /> class.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
        ///     data about the exception being thrown.
        /// </param>
        /// <param name="context">
        ///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
        ///     information about the source or destination.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info" /> parameter is null. </exception>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">
        ///     The class name is null or
        ///     <see cref="P:System.Exception.HResult" /> is zero (0).
        /// </exception>
        protected TransactionsAlreadyImportedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}