using System.Text.Json.Serialization;
using AethersenseReduxReborn.Signals.Configs;
using Buttplug.Core.Messages;

namespace AethersenseReduxReborn.Buttplug.Configs;

public class DeviceActuatorConfig
{
    public required uint         Index        { get; init; }
    public required ActuatorType ActuatorType { get; init; }
    public required string       Description  { get; init; }
    public required uint         Steps        { get; init; }
    public required ActuatorHash Hash         { get; init; }
    public          CombineType  CombineType  { get; init; } = CombineType.Max;
    [JsonIgnore]
    public string DisplayName => $"{Index} - {ActuatorType} - {Description}";
}
