using System;

namespace BudgetAnalyser.Engine
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class AutoRegisterWithIoCAttribute : Attribute
    {
        public string Named { get; set; }
        public bool SingleInstance { get; set; }
    }
}