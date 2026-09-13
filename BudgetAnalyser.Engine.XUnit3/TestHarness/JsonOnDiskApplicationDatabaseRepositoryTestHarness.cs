using System.Text;
using BudgetAnalyser.Engine.Persistence;
using Rees.UnitTestUtilities;

namespace BudgetAnalyser.Engine.XUnit.TestHarness;

public class JsonOnDiskApplicationDatabaseRepositoryTestHarness(IDtoMapper<BudgetAnalyserStorageRoot2, ApplicationDatabase> mapper, ILogger logger)
    : JsonOnDiskApplicationDatabaseRepository(mapper, logger), IDisposable
{
    public BudgetAnalyserStorageRoot2? Dto { get; private set; }
    public Func<string, bool>? FileExistsOverride { get; set; }

    public Stream? ReadableStream { get; private set; }

    public string SerialisedData { get; private set; } = string.Empty;

    public Stream WritableStream { get; } = new MemoryStream();

    public void Dispose()
    {
        ReadableStream?.Dispose();
        WritableStream?.Dispose();
    }

    protected override Stream CreateReadableStream(string fileName)
    {
        if (ReadableStream is not null)
        {
            return ReadableStream;
        }

        SerialisedData = GetType().Assembly.ExtractEmbeddedResourceAsText(fileName);
        ReadableStream = new MemoryStream(Encoding.UTF8.GetBytes(SerialisedData));
        return ReadableStream;
    }

    protected override Stream CreateWritableStream(string fileName)
    {
        return WritableStream;
    }

    protected override bool FileExists(string budgetAnalyserDataStorage)
    {
        return FileExistsOverride?.Invoke(budgetAnalyserDataStorage) ?? true;
    }

    protected override async Task<BudgetAnalyserStorageRoot2> LoadJsonFromDiskAsync(string fileName)
    {
        Dto = await base.LoadJsonFromDiskAsync(fileName);
        return Dto;
    }

    protected override BudgetAnalyserStorageRoot2 MapToDto(ApplicationDatabase model)
    {
        Dto = base.MapToDto(model);
        return Dto;
    }

    protected override async Task SerialiseAndWriteToStream(Stream stream, BudgetAnalyserStorageRoot2 dto)
    {
        await base.SerialiseAndWriteToStream(stream, dto);
        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var result = await reader.ReadToEndAsync();
        SerialisedData = result;
    }
}
