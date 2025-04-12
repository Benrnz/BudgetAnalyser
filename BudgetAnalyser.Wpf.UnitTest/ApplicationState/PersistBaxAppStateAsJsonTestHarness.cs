using BudgetAnalyser.ApplicationState;
using JetBrains.Annotations;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.Wpf.UnitTest.ApplicationState;

public class PersistBaxAppStateAsJsonTestHarness([NotNull] IUserMessageBox userMessageBox) : PersistBaxAppStateAsJson(userMessageBox)
{
    public string SerialisedData { get; set; } = string.Empty;

    protected override Stream CreateWritableStream()
    {
        return new MemoryStream();
    }

    protected override string DefaultFileName()
    {
        return "DefaultFileName.json";
    }

    protected override string ReadAppStateFileAsText()
    {
        return SerialisedData;
    }

    protected override bool CheckFileNameExists()
    {
        return true;
    }

    protected override void WriteToStream(Stream stream, BaxApplicationStateDto data)
    {
        base.WriteToStream(stream, data);
        stream.Position = 0;
        using var reader = new StreamReader(stream);
        SerialisedData = reader.ReadToEnd();
    }
}
