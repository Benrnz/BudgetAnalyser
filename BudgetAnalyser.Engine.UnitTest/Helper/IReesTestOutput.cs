namespace BudgetAnalyser.Engine.UnitTest.Helper;

public interface IReesTestOutput
{
    void Write(string text);
    void Write(string template, params object[] args);
    void WriteLine(string line);
    void WriteLine(string template, params object[] args);
}
