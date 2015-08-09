using System.ComponentModel;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Widgets
{
    public interface IWidgetWithAdditionalImage : INotifyPropertyChanged
    {
        [UsedImplicitly]
        string ImageResourceName2 { get; set; }
    }
}