using System;
using System.IO;
using System.Runtime.Serialization;

namespace Rees.Wpf.ApplicationState
{
    /// <summary>
    ///     An exception used when a bad Application State file is read that is not compatible with this application. Might
    ///     indicate tampering, or an old now unsupported file version.
    /// </summary>
    [Serializable]
    public class BadApplicationStateFileFormatException : IOException
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BadApplicationStateFileFormatException" /> class.
        /// </summary>
        public BadApplicationStateFileFormatException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BadApplicationStateFileFormatException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public BadApplicationStateFileFormatException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BadApplicationStateFileFormatException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public BadApplicationStateFileFormatException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BadApplicationStateFileFormatException" /> class.
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
        protected BadApplicationStateFileFormatException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}