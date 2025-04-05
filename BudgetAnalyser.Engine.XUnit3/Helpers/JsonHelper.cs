using System.Text.Json;

namespace BudgetAnalyser.Engine.XUnit.Helpers;

public static class JsonHelper
{
    public static string MinifyJson(string jsonString)
    {
        using var document = JsonDocument.Parse(jsonString);
        return JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = false });
    }
}
