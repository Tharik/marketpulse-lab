using System.Text.Json;
using System.Text.Json.Serialization;

namespace MarketPulse.Contracts.Serialization;

public static class EventJsonSerializerOptions
{
    public static JsonSerializerOptions Default { get; } = Create();

    private static JsonSerializerOptions Create()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = false,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };
    }
}
