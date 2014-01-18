using System;

namespace BudgetAnalyser.Engine
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class AutoRegisterWithIoCAttribute : Attribute
    {
        public string Named { get; set; }
        public bool SingleInstance { get; set; }
    }
}