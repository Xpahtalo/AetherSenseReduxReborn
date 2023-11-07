using System.Collections.Generic;

namespace AethersenseReduxReborn.Buttplug.Configs;

public class DeviceConfig
{
    public required string                     Name      { get; set; }
    public required List<DeviceActuatorConfig> Actuators { get; set; }
}
