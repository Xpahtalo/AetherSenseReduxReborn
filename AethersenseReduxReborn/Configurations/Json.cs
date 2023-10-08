using System.Text.Json;
using System.Text.Json.Serialization;

namespace AethersenseReduxReborn.Configurations;

public static class Json
{
    private static JsonSerializerOptions? _options;
    public static JsonSerializerOptions Options => _options ??= new JsonSerializerOptions {
        WriteIndented = true,
        Converters = {
            new JsonStringEnumConverter(),
        },
    };
}
