using System.Text;


namespace BudgetAnalyser.Engine.XUnit.Helpers;

public class XUnitOutputWriter(ITestOutputHelper xunitOutput) : IReesTestOutput, IDisposable
{
    private StringBuilder? lineBuilder = new();

    public void Dispose()
    {
        if (this.lineBuilder is not null)
        {
            xunitOutput.WriteLine(this.lineBuilder.ToString());
            this.lineBuilder = null;
        }
    }

    public void Write(string text)
    {
        if (this.lineBuilder == null)
        {
            this.lineBuilder = new StringBuilder(text);
        }
        else
        {
            this.lineBuilder.Append(text);
        }
    }

    public void Write(string template, params object[] args)
    {
        if (this.lineBuilder == null)
        {
            this.lineBuilder = new StringBuilder(string.Format(template, args));
        }
        else
        {
            this.lineBuilder.Append(string.Format(template, args));
        }
    }

    public void WriteLine(string line)
    {
        if (this.lineBuilder is not null)
        {
            xunitOutput.WriteLine(this.lineBuilder + line);
            this.lineBuilder = null;
        }
        else
        {
            xunitOutput.WriteLine(line);
        }
    }

    public void WriteLine(string template, params object[] args)
    {
        if (this.lineBuilder is not null)
        {
            xunitOutput.WriteLine(this.lineBuilder + string.Format(template, args));
            this.lineBuilder = null;
        }
        else
        {
            xunitOutput.WriteLine(template, args);
        }
    }
}
