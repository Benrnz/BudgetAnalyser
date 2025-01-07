using System.Diagnostics;

namespace BudgetAnalyser.Engine.UnitTest.Helper;

public class DebugTestOutput : IReesTestOutput
{
    public void Write(string text)
    {
        Debug.Write(text);
    }

    public void Write(string template, params object[] args)
    {
        Debug.Write(string.Format(template, args));
    }

    public void WriteLine(string line)
    {
        Debug.WriteLine(line);
    }

    public void WriteLine(string template, params object[] args)
    {
        Debug.WriteLine(template, args);
    }
}
