using System.Text.Json.Serialization;

namespace AethersenseReduxReborn.Signals;

[JsonDerivedType(typeof(ChatTriggerSignalConfig),     "ChatTrigger")]
[JsonDerivedType(typeof(CharacterAttributeSignalConfig), "PlayerAttribute")]
public abstract class SignalSourceConfig
{
    public string Name { get; set; } = "New Signal Source";
}
