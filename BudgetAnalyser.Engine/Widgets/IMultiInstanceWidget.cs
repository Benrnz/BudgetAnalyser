using System;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     Widgets that implement this interface, will have multiple instances created to show different statuses.  Each
    ///     instance must be uniquely
    ///     identified by the <see cref="Id" /> property combined with the <see cref="WidgetType" /> Property.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Preferred Spelling")]
    public interface IMultiInstanceWidget
    {
        string Id { get; set; }

        bool Visibility { get; set; }

        Type WidgetType { get; }
    }
}