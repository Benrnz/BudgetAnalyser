using System;

namespace Rees.TangyFruitMapper.Validation
{
    /// <summary>
    ///     An exception that will be used when Dto Properties are found to be non-public or non-writable.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class PropertiesMustBePublicWriteableException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PropertiesMustBePublicWriteableException" /> class.
        /// </summary>
        public PropertiesMustBePublicWriteableException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PropertiesMustBePublicWriteableException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public PropertiesMustBePublicWriteableException(string message) : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PropertiesMustBePublicWriteableException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public PropertiesMustBePublicWriteableException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}