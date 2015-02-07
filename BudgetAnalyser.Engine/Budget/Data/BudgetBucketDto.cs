using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Budget.Data
{
    public class BudgetBucketDto
    {
        public bool Active { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Permissable in this case as it is linked to the type.")]
        public virtual BucketDtoType Type { get; set; }
    }
}