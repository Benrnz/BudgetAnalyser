namespace BudgetAnalyser.Engine;

internal interface ICloneable<out T>
{
    T Clone();
}
