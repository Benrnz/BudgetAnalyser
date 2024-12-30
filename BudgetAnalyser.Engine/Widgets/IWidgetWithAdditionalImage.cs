using System.ComponentModel;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     Indicates that the implementation has an additional image that can be used.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public interface IWidgetWithAdditionalImage : INotifyPropertyChanged
    {
        /// <summary>
        ///     Gets or sets the secondary image resource name.
        /// </summary>
        [UsedImplicitly]
        string ImageResourceName2 { get; set; }
    }
}
