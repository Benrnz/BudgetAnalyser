using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BudgetAnalyser.Engine.Ledger.Data;

namespace BudgetAnalyser.Engine.Persistence
{
    /// <summary>
    ///     A Dto object to store the top level Budget Analyser database file.
    /// </summary>
    public class BudgetAnalyserStorageRoot
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BudgetAnalyserStorageRoot" /> class.
        /// </summary>
        public BudgetAnalyserStorageRoot()
        {
            LedgerReconciliationToDoCollection = new List<ToDoTaskDto>();
        }

        /// <summary>
        ///     Gets or sets the budget collection root dto.
        /// </summary>
        public StorageBranch BudgetCollectionRootDto { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the data files are encrypted.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is encrypted; otherwise, <c>false</c>.
        /// </value>
        public bool IsEncrypted { get; set; }

        /// <summary>
        ///     Gets or sets the ledger book root dto.
        /// </summary>
        public StorageBranch LedgerBookRootDto { get; set; }

        /// <summary>
        ///     Gets or sets the ledger reconciliation to-do task collection.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "Required for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists",
            Justification = "Required for serialisation")]
        public List<ToDoTaskDto> LedgerReconciliationToDoCollection { get; set; }

        /// <summary>
        ///     Gets or sets the matching rules collection root dto.
        /// </summary>
        public StorageBranch MatchingRulesCollectionRootDto { get; set; }

        /// <summary>
        ///     Gets or sets the statement model root dto.
        /// </summary>
        public StorageBranch StatementModelRootDto { get; set; }
    }
}