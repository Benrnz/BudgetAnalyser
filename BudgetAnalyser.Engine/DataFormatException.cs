using System;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     The data format has been determined invalid while trying to parse it into the object model.
    ///     The underlying problem could be a serialisation issue or corrupt data.
    /// </summary>
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
    }
}