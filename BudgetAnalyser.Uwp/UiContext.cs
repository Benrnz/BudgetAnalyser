using BudgetAnalyser.Engine;

namespace BudgetAnalyser.Uwp
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class UiContext
    {
        public virtual ILogger Logger { get; set; }
    }
}
