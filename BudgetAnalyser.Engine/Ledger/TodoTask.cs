namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    /// A task item for use with the <see cref="TodoList"/>.
    /// </summary>
    public class TodoTask
    {
        public TodoTask(string description, bool systemGenerated = false)
        {
            Description = description;
            SystemGenerated = systemGenerated;
        }

        public string Description { get; private set; }

        public bool SystemGenerated { get; private set; }
    }
}