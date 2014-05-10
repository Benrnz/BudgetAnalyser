using System;
using System.Runtime.Serialization;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    ///     An exception to represent an index out of range exception when parsing a Bank Statement CSV file.
    /// </summary>
    [Serializable]
    public class UnexpectedIndexException : Exception
    {
        public UnexpectedIndexException()
        {
        }

        public UnexpectedIndexException(string message) : base(message)
        {
        }

        [UsedImplicitly]
        public UnexpectedIndexException(string message, Exception innerException) : base(message, innerException)
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
        protected UnexpectedIndexException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}