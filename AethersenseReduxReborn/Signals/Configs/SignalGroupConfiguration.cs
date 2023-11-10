using System.Collections.Generic;
using AethersenseReduxReborn.Buttplug;

namespace AethersenseReduxReborn.Signals.Configs;

public class SignalGroupConfiguration
{
    public required string                   Name                       { get; set; }
    public required CombineType              CombineType                { get; set; }
    public required List<SignalSourceConfig> SignalSources              { get; set; }
    public          ActuatorHash             HashOfLastAssignedActuator { get; set; } = ActuatorHash.Zeroed;
}
