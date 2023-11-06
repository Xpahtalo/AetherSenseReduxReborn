using System.Collections.Generic;

namespace AethersenseReduxReborn.Buttplug.Configs;

public class DeviceConfig
{
    public string                     Name      { get; set; }
    public List<DeviceActuatorConfig> Actuators { get; set; }
}
