namespace BudgetAnalyser.Engine.Services;

public class DependencyChangedEventArgs(Type dependencyType) : EventArgs
{
    public Type DependencyType { get; private set; } = dependencyType;
}
