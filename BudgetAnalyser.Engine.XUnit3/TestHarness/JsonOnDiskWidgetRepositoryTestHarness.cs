using System.Text;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.Engine.Widgets.Data;

namespace BudgetAnalyser.Engine.XUnit.TestHarness;

internal class JsonOnDiskWidgetRepositoryTestHarness : JsonOnDiskWidgetRepository
{
    private readonly EmbeddedResourceFileReaderWriterEncrypted? encryptedReaderWriter;

    public JsonOnDiskWidgetRepositoryTestHarness(ILogger logger, IReaderWriterSelector readerWriterSelector)
        : base(new MapperWidgetToDto(new WidgetCatalog()), logger, readerWriterSelector, new WidgetCatalog())
    {
        Dto = Array.Empty<WidgetDto>();
        SerialisedData = string.Empty;
        SerialisedBytes = [];
        if (readerWriterSelector.SelectReaderWriter(true) is EmbeddedResourceFileReaderWriterEncrypted encryptor)
        {
            this.encryptedReaderWriter = encryptor;
        }
    }

    public IEnumerable<WidgetDto> Dto { get; set; }

    public bool IsEncryptedAtLastAccess { get; private set; }

    public byte[] SerialisedBytes { get; private set; }

    public string SerialisedData { get; private set; }

    protected override async Task<List<WidgetDto>> LoadJsonFromDiskAsync(string fileName, bool isEncrypted)
    {
        Dto = await base.LoadJsonFromDiskAsync(fileName, isEncrypted);
        return Dto.ToList();
    }

    protected override IEnumerable<WidgetDto> MapToDto(IEnumerable<Widget> widgets)
    {
        Dto = base.MapToDto(widgets);
        return Dto;
    }

    protected override Task SaveToDiskAsync(string fileName, IEnumerable<WidgetDto> dataEntities, bool isEncrypted)
    {
        IsEncryptedAtLastAccess = isEncrypted;
        return base.SaveToDiskAsync(fileName, dataEntities, isEncrypted);
    }

    protected override async Task SerialiseAndWriteToStream(Stream stream, IEnumerable<WidgetDto> dataEntities)
    {
        await base.SerialiseAndWriteToStream(stream, dataEntities);
        stream.Position = 0;
        if (IsEncryptedAtLastAccess)
        {
            var encryptedDestinationStream = this.encryptedReaderWriter!.OutputStream;
            encryptedDestinationStream.Position = 0;
            using var byteReader = new BinaryReader(encryptedDestinationStream);
            SerialisedBytes = byteReader.ReadBytes((int)encryptedDestinationStream.Length);
            SerialisedData = string.Empty;
        }
        else
        {
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var result = await reader.ReadToEndAsync();
            SerialisedData = result;
            SerialisedBytes = [];
        }
    }
}
