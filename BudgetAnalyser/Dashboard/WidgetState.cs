namespace BudgetAnalyser.Dashboard
{
    /// <summary>
    /// Application state stored for a single widget.
    /// </summary>
    public class WidgetState
    {
        public bool Visible { get; set; }
        public string Id { get; set; }
        public IdentifierType IdType { get; set; }
    }
}