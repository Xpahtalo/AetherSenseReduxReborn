using System.Text.Json.Serialization;
using Buttplug.Core.Messages;

namespace AethersenseReduxReborn.Buttplug.Configs;

public class DeviceActuatorConfig
{
    public uint         Index        { get; set; }
    public ActuatorType ActuatorType { get; set; }
    public string       Description  { get; set; }
    public uint         Steps        { get; set; }
    public ActuatorHash Hash         { get; set; }
    [JsonIgnore]
    public string DisplayName => $"{Index} - {ActuatorType} - {Description}";
}
