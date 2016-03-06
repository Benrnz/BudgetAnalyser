using BudgetAnalyser.Engine;

namespace BudgetAnalyser.Uwp
{
    [AutoRegisterWithIoC]
    public static class StaticBinder
    {
        [PropertyInjection]
        public static UiContext UiContext { get; set; }
    }
}
