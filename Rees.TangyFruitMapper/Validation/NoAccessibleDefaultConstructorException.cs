using System;

namespace Rees.TangyFruitMapper.Validation
{
    /// <summary>
    ///     An exception that will be used if Dto and Model types do not have an accessable default constructor.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class NoAccessibleDefaultConstructorException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NoAccessibleDefaultConstructorException" /> class.
        /// </summary>
        public NoAccessibleDefaultConstructorException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoAccessibleDefaultConstructorException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public NoAccessibleDefaultConstructorException(string message) : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoAccessibleDefaultConstructorException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public NoAccessibleDefaultConstructorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}