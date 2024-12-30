using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using NotNull = JetBrains.Annotations.NotNullAttribute;

namespace BudgetAnalyser.Engine.Ledger.Reconciliation
{
    /// <summary>
    ///     This class is responsible for validating that changes were made during a reconciliation that did not alter the
    ///     Ledger Book in an inappropriate and invalid way.
    /// </summary>
    public sealed class ReconciliationConsistencyChecker : IDisposable
    {
        private readonly decimal check1;

        private readonly LedgerBook ledgerBook;
        private decimal check2;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReconciliationConsistencyChecker" /> class.
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        public ReconciliationConsistencyChecker([NotNull] LedgerBook book)
        {
            this.ledgerBook = book ?? throw new ArgumentNullException(nameof(book));
            this.check1 = this.ledgerBook.Reconciliations.Sum(e => e.CalculatedSurplus);
        }

        /// <summary>
        ///     Used in this case to perform finalising logic to check consistency of reconciliation changes.
        /// </summary>
        /// <exception cref="CorruptedLedgerBookException">
        ///     Code Error: The previous dated entries have changed, this is not
        ///     allowed. Data is corrupt.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "Allowed here, using syntax only")]
        [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly", Justification = "Not required here, using syntax only")]
        public void Dispose()
        {
            this.check2 = this.ledgerBook.Reconciliations.Sum(e => e.CalculatedSurplus) - this.ledgerBook.Reconciliations.First().CalculatedSurplus;
            if (this.check1 != this.check2)
            {
                throw new CorruptedLedgerBookException("Code Error: The previous dated entries have changed, this is not allowed. Data is corrupt.");
            }
        }
    }
}