using System;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    ///     An exception to represent an inconsistency in the <see cref="StatementModel" /> loaded. The check sum does not
    ///     match the data.
    /// </summary>
    public class StatementModelChecksumException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatementModelChecksumException"/> class.
        /// </summary>
        public StatementModelChecksumException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatementModelChecksumException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
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
        /// Gets the file checksum hash.
        /// </summary>
        public string FileChecksum { get; }
    }
}