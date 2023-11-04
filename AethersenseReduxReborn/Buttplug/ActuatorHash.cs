using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using AethersenseReduxReborn.Misc;
using Buttplug.Core.Messages;

namespace AethersenseReduxReborn.Buttplug;

public record ActuatorHash: Md5Hash
{
    public new static ActuatorHash Zeroed { get; }

    static ActuatorHash() { Zeroed = (Md5Hash.Zeroed as ActuatorHash)!; }

    [JsonConstructor]
    public ActuatorHash(ImmutableArray<byte> value)
        : base(value) { }

    [SetsRequiredMembers]
    public ActuatorHash(DeviceActuator actuator)
        : this($"{actuator.OwnerDevice.Name}"
             + $"{actuator.Index}"
             + $"{actuator.ActuatorType}"
             + $"{actuator.Description}"
             + $"{actuator.Steps}") { }

    [SetsRequiredMembers]
    private ActuatorHash(string hashString)
        : base(hashString) { }

    public static ActuatorHash FromInternalAttribute(GenericDeviceMessageAttributes attribute, string deviceName) =>
        new($"{deviceName}"
          + $"{attribute.Index}"
          + $"{attribute.ActuatorType}"
          + $"{attribute.FeatureDescriptor}"
          + $"{attribute.StepCount}");

    public override string ToString() => base.ToString();
}
