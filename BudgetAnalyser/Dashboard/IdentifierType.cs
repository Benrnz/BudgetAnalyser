namespace BudgetAnalyser.Dashboard
{
    /// <summary>
    /// An enum to describe what kind of identifier is stored in the <see cref="WidgetState.Id"/> Property.
    /// </summary>
    public enum IdentifierType
    {
        /// <summary>
        /// The Id Property represents a full type name.  Only one instance of this type of widget will be constructed.
        /// </summary>
        FullTypeName,
    }
}