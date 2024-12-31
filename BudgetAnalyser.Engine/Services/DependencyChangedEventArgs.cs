using System;

namespace BudgetAnalyser.Engine.Services
{
    internal class DependencyChangedEventArgs : EventArgs
    {
        public DependencyChangedEventArgs(Type dependencyType)
        {
            DependencyType = dependencyType;
        }

        public Type DependencyType { get; private set; }
    }
}
