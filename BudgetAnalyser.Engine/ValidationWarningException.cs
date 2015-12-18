using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     The file format has been determined invlaid while trying to parse it into the object model.
    /// </summary>
    public class ValidationWarningException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidationWarningException" /> class.
        /// </summary>
        [UsedImplicitly]
        public ValidationWarningException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidationWarningException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ValidationWarningException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidationWarningException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ValidationWarningException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}