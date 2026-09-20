using System.Text.Json;

namespace BudgetAnalyser.Engine.Persistence;

/// <summary>
///     Shared load/save/serialise plumbing for the JSON-on-disk repositories that read and write a single
///     serialised <typeparamref name="TDto" /> through an <see cref="IReaderWriterSelector" />. Each repository
///     keeps its own public interface, business logic and DTO mapping; only the stream and (de)serialisation
///     layer lives here.
/// </summary>
/// <typeparam name="TDto">The exact type serialised to and deserialised from disk.</typeparam>
public abstract class JsonOnDiskRepositoryBase<TDto>
{
    private static readonly JsonSerializerOptions ReadOptions = new();

    protected JsonOnDiskRepositoryBase(IReaderWriterSelector readerWriterSelector)
    {
        ReaderWriterSelector = readerWriterSelector ?? throw new ArgumentNullException(nameof(readerWriterSelector));
    }

    protected IReaderWriterSelector ReaderWriterSelector { get; }

    protected virtual async Task<TDto> LoadJsonFromDiskAsync(string fileName, bool isEncrypted)
    {
        var reader = ReaderWriterSelector.SelectReaderWriter(isEncrypted);
        await using var stream = reader.CreateReadableStream(fileName);
        var dto = await JsonSerializer.DeserializeAsync<TDto>(stream, ReadOptions);

        return dto ?? throw CreateCorruptFileException();
    }

    protected virtual async Task SaveToDiskAsync(string fileName, TDto dataEntity, bool isEncrypted)
    {
        if (dataEntity is null)
        {
            throw new ArgumentNullException(nameof(dataEntity));
        }

        var writer = ReaderWriterSelector.SelectReaderWriter(isEncrypted);
        await using var stream = writer.CreateWritableStream(fileName);
        await SerialiseAndWriteToStream(stream, dataEntity);
    }

    protected virtual async Task SerialiseAndWriteToStream(Stream stream, TDto dataEntity)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        await JsonSerializer.SerializeAsync(stream, dataEntity, options);
    }

    /// <summary>Creates the exception to throw when the deserialised file is unexpectedly empty.</summary>
    protected abstract Exception CreateCorruptFileException();
}
