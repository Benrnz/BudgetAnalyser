using System;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     An exception to report data corruption issues in the LedgerBook data.
    /// </summary>
    public class CorruptedLedgerBookException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CorruptedLedgerBookException" /> class.
        /// </summary>
        [UsedImplicitly]
        public CorruptedLedgerBookException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CorruptedLedgerBookException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public CorruptedLedgerBookException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CorruptedLedgerBookException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public CorruptedLedgerBookException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}