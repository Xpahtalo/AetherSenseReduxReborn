using System.Text.Json;

namespace AethersenseReduxReborn.Configurations;

public sealed class ButtplugPluginConfiguration
{
    public int    Version { get; set; } = 1;
    public string Address { get; set; } = "ws://127.0.0.1:12345";

    public SignalPluginConfiguration DeepCopy() => JsonSerializer.Deserialize<SignalPluginConfiguration>(JsonSerializer.Serialize(this, Json.Options), Json.Options)!;
}
