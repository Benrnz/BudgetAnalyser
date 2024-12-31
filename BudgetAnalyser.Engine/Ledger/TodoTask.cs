namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     A task item for use with the <see cref="ToDoCollection" />.
    /// </summary>
    public class ToDoTask
    {
        /// <summary>
        ///     Used only for persistence. If you're consuming this outside a Mapper, you're using this constructor incorrectly.
        /// </summary>
        internal ToDoTask()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ToDoTask" /> class.
        /// </summary>
        public ToDoTask(string description, bool systemGenerated = false, bool canDelete = true)
        {
            Description = description;
            SystemGenerated = systemGenerated;
            CanDelete = canDelete;
        }

        /// <summary>
        ///     Gets a value indicating whether this task can be deleted by the user.
        /// </summary>
        public bool CanDelete { get; internal set; }

        /// <summary>
        ///     Gets the description of the task.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether the task is system generated.
        /// </summary>
        public bool SystemGenerated { get; private set; }
    }
}
