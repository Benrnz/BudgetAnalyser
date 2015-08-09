using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    public class ToDoTaskDto
    {
        public bool CanDelete { [UsedImplicitly] get; set; }
        public string Description { [UsedImplicitly] get; set; }
        public bool SystemGenerated { [UsedImplicitly] get; set; }
    }
}