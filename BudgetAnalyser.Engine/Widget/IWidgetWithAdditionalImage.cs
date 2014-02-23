using System.ComponentModel;

namespace BudgetAnalyser.Engine.Widget
{
    public interface IWidgetWithAdditionalImage : INotifyPropertyChanged
    {
        string ImageResourceName2 { get; set; }
    }
}