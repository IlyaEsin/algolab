using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AlgoLab.Core.Json;

public static class AlgoLabJson
{
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = false,
    };

    /// <summary>Человекочитаемое представление входа или результата для UI и для сравнения.</summary>
    public static string Describe<T>(T value) => JsonSerializer.Serialize(value, Options);
}
