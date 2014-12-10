using System;
using System.Runtime.Serialization;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     The data format has been determined invalid while trying to parse it into the object model.
    ///     The underlying problem could be a serialisation issue or corrupt data.
    /// </summary>
    [Serializable]
    public class DataFormatException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DataFormatException" /> class.
        /// </summary>
        public DataFormatException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataFormatException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public DataFormatException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataFormatException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public DataFormatException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataFormatException" /> class.
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
        protected DataFormatException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}