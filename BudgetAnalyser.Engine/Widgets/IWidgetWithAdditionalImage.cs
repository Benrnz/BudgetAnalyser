using System.ComponentModel;

namespace BudgetAnalyser.Engine.Widgets
{
    public interface IWidgetWithAdditionalImage : INotifyPropertyChanged
    {
        string ImageResourceName2 { get; set; }
    }
}