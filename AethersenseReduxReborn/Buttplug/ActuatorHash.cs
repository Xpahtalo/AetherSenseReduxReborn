using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using AethersenseReduxReborn.Misc;
using Buttplug.Core.Messages;

namespace AethersenseReduxReborn.Buttplug;

public record ActuatorHash: Md5Hash
{
    public new static ActuatorHash Zeroed { get; }

    static ActuatorHash() { Zeroed = (Md5Hash.Zeroed as ActuatorHash)!; }

    [SetsRequiredMembers]
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

    [SetsRequiredMembers]
    public ActuatorHash(GenericDeviceMessageAttributes attribute, string deviceName)
        : this($"{deviceName}"
             + $"{attribute.Index}"
             + $"{attribute.ActuatorType}"
             + $"{attribute.FeatureDescriptor}"
             + $"{attribute.StepCount}") { }

    public override string ToString() => base.ToString();
}

public class ActuatorHashConverter: JsonConverter<ActuatorHash>
{
    public override ActuatorHash? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => new(reader.GetBytesFromBase64().ToImmutableArray());

    public override void Write(Utf8JsonWriter writer, ActuatorHash value, JsonSerializerOptions options) { writer.WriteBase64StringValue(value.Value.ToArray()); }
}
