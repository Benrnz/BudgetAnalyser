using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.UnitTest
{
    [AutoRegisterWithIoC]
    [UsedImplicitly]
    public static class AutoRegisterWithIoCProcessorPropertyInjectionTestSource
    {
        [PropertyInjection]
        public static ILogger Logger { get; set; }

        public static ILogger NotInjected { get; set; }
    }
}
