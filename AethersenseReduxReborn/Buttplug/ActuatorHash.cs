using System;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Buttplug.Core.Messages;

namespace AethersenseReduxReborn.Buttplug;

public readonly struct ActuatorHash: IEquatable<ActuatorHash>
{
    public required ImmutableArray<byte> Value { get; init; }

    public static ActuatorHash Unassigned { get; }

    static ActuatorHash()
    {
        Unassigned = new ActuatorHash {
            Value = ImmutableArray.CreateRange(new byte[16]),
        };
    }

    private static ActuatorHash ComputeHash(string deviceName, uint index, ActuatorType actuatorType, string description, uint steps)
    {
        var hashString = $"{deviceName}{index}{actuatorType}{description}{steps}";
        var hash       = MD5.HashData(Encoding.UTF8.GetBytes(hashString));
        Service.PluginLog.Debug("Computed hash {0} for actuator {1}", BitConverter.ToString(hash), hashString);
        return new ActuatorHash {
            Value = ImmutableArray.CreateRange(hash),
        };
    }

    public static ActuatorHash ComputeHash(DeviceActuator actuator) => ComputeHash(actuator.OwnerDevice.Name, actuator.Index, actuator.ActuatorType, actuator.Description, actuator.Steps);

#region Overrides

    public override bool Equals(object? obj) => obj is ActuatorHash other && this == other;

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(ActuatorHash left, ActuatorHash right) => left.Value.AsSpan().SequenceEqual(right.Value.AsSpan());

    public static bool operator !=(ActuatorHash left, ActuatorHash right) => !(left == right);

    public bool Equals(ActuatorHash other) => this == other;

    public override string ToString() => this == Unassigned ? "Unassigned" : BitConverter.ToString(Value.ToArray());

#endregion
}
