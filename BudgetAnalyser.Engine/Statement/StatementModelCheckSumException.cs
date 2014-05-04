using System;
using System.Runtime.Serialization;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    ///     An exception to represent an inconsistency in the <see cref="StatementModel" /> loaded. The check sum does not
    ///     match the data.
    /// </summary>
    [Serializable]
    public class StatementModelChecksumException : Exception
    {
        public StatementModelChecksumException() : base()
        {
        }

        [UsedImplicitly]
        public StatementModelChecksumException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="StatementModelChecksumException" /> class.
        /// </summary>
        /// <param name="checksum">The actual checksum of the file.</param>
        public StatementModelChecksumException(string checksum)
        {
            FileChecksum = checksum;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="StatementModelChecksumException" /> class.
        /// </summary>
        /// <param name="checksum">The actual checksum of the file.</param>
        /// <param name="message">The message.</param>
        public StatementModelChecksumException(string checksum, string message)
            : base(message)
        {
            FileChecksum = checksum;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="StatementModelChecksumException" /> class.
        /// </summary>
        /// <param name="checksum">The actual checksum of the file.</param>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public StatementModelChecksumException(string checksum, string message, Exception innerException)
            : base(message, innerException)
        {
            FileChecksum = checksum;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="StatementModelChecksumException" /> class.
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
        protected StatementModelChecksumException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string FileChecksum { get; private set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("FileChecksum", FileChecksum);
        }
    }
}