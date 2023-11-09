using System.Text.Json.Serialization;
using AethersenseReduxReborn.Signals.Configs;
using Buttplug.Core.Messages;

namespace AethersenseReduxReborn.Buttplug.Configs;

public class DeviceActuatorConfig
{
    public required uint         Index        { get; set; }
    public required ActuatorType ActuatorType { get; set; }
    public required string       Description  { get; set; }
    public required uint         Steps        { get; set; }
    public required ActuatorHash Hash         { get; set; }
    public          CombineType  CombineType  { get; set; } = CombineType.Max;
    [JsonIgnore]
    public string DisplayName => $"{Index} - {ActuatorType} - {Description}";
}
