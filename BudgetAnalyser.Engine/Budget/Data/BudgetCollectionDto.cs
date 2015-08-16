using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Necessary for convention")]
    public class BudgetCollectionDto
    {
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Necessary for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Necessary for serialisation")]
        public List<BudgetBucketDto> Buckets { get; [UsedImplicitly] set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Necessary for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Necessary for serialisation")]
        public List<BudgetModelDto> Budgets { get; set; }

        public string FileName { get; set; }
    }
}