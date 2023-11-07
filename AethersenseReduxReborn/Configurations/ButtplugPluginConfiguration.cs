using System.Collections.Generic;
using AethersenseReduxReborn.Buttplug.Configs;

namespace AethersenseReduxReborn.Configurations;

public sealed class ButtplugPluginConfiguration
{
    public int                Version      { get; set; } = 1;
    public string             Address      { get; set; } = "ws://127.0.0.1:12345";
    public List<DeviceConfig> SavedDevices { get; set; } = new();
}
