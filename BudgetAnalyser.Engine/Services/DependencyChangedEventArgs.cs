namespace BudgetAnalyser.Engine.Services;

internal class DependencyChangedEventArgs(Type dependencyType) : EventArgs
{
    public Type DependencyType { get; private set; } = dependencyType;
}
