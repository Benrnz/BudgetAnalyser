using System;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    ///     An exception that describes an attempt to open an encrypted file without any credentials provided.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class EncryptionKeyNotProvidedException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EncryptionKeyNotProvidedException" /> class.
        /// </summary>
        public EncryptionKeyNotProvidedException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EncryptionKeyNotProvidedException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public EncryptionKeyNotProvidedException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EncryptionKeyNotProvidedException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public EncryptionKeyNotProvidedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}