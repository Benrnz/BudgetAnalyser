using System;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Budget
{
    /// <summary>
    ///     An exception use to communicate problems with using, or maintaining budgets.
    /// </summary>
    public class BudgetException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BudgetException" /> class.
        /// </summary>
        [UsedImplicitly]
        public BudgetException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BudgetException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public BudgetException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BudgetException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public BudgetException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}