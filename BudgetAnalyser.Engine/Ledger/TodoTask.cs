namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    /// A task item for use with the <see cref="ToDoCollection"/>.
    /// </summary>
    public class ToDoTask
    {
        public ToDoTask(string description, bool systemGenerated = false)
        {
            Description = description;
            SystemGenerated = systemGenerated;
        }

        public string Description { get; private set; }

        public bool SystemGenerated { get; private set; }
    }
}