using System;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     An exception that describes a problem with duplicate data , where no duplicates are expected.
    /// </summary>
    public class DuplicateNameException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DuplicateNameException" /> class.
        /// </summary>
        public DuplicateNameException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DuplicateNameException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public DuplicateNameException(string message) : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DuplicateNameException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public DuplicateNameException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}