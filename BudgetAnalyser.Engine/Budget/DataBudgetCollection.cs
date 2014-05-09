using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Budget
{
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Necessary for convention")]
    public class DataBudgetCollection
    {
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Necessary for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Necessary for serialisation")]
        public List<DataBudgetModel> Budgets { get; set; }

        public string FileName { get; set; }
    }
}