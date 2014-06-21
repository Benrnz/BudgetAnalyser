using System;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    /// An attribute to mark any static property on any static class to be injected on application start up. This will occur after all other IoC functions have occured.
    /// Only static classes and static properties are supported because property injection should be discouraged. It is sometimes necessary for WPF UI binding.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PropertyInjectionAttribute : Attribute
    {
    }
}
