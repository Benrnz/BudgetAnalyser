using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    /// <summary>
    ///     A Dto object to persist a collection of budgets.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix",
        Justification = "Necessary for convention")]
    public class BudgetCollectionDto
    {
        /// <summary>
        ///     Gets or sets the buckets included in the buget collection.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "Necessary for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists",
            Justification = "Necessary for serialisation")]
        public List<BudgetBucketDto> Buckets { get; [UsedImplicitly] set; }

        /// <summary>
        ///     Gets or sets the budgets included in the budget collection.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "Necessary for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists",
            Justification = "Necessary for serialisation")]
        public List<BudgetModelDto> Budgets { get; set; }

        /// <summary>
        ///     Gets or sets the storage key.
        /// </summary>
        public string StorageKey { get; set; }
    }
}
