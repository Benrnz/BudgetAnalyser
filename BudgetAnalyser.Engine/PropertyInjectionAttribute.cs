using System;

namespace BudgetAnalyser.Engine
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PropertyInjectionAttribute : Attribute
    {
    }
}
