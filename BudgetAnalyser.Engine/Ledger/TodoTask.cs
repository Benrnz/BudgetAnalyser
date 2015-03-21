namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     A task item for use with the <see cref="ToDoCollection" />.
    /// </summary>
    public class ToDoTask
    {
        public ToDoTask(string description, bool systemGenerated = false, bool canDelete = true)
        {
            Description = description;
            SystemGenerated = systemGenerated;
            CanDelete = canDelete;
            TaskType = ToDoTaskType.Other;
        }

        public decimal Amount { get; internal set; }
        public bool CanDelete { get; internal set; }
        public string Description { get; private set; }
        public bool SystemGenerated { get; private set; }
        internal ToDoTaskType TaskType { get; set; }
    }
}