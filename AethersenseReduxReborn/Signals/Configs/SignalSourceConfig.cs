using System.Text.Json.Serialization;

namespace AethersenseReduxReborn.Signals.Configs;

[JsonDerivedType(typeof(ChatTriggerSignalConfig),        "ChatTrigger")]
[JsonDerivedType(typeof(CharacterAttributeSignalConfig), "CharacterAttribute")]
public abstract class SignalSourceConfig
{
    public required string Name { get; set; }
}
