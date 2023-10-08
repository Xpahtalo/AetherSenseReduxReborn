using System.Collections.Generic;

namespace AethersenseReduxReborn.Signals.SignalGroup;

public class SignalGroupConfiguration
{
    public required string                   Name                       { get; set; }
    public required CombineType              CombineType                { get; set; }
    public required List<SignalSourceConfig> SignalSources              { get; set; }
    public          byte[]?                  HashOfLastAssignedActuator { get; set; }
}

public enum CombineType
{
    Average,
    Max,
    Minimum,
}
