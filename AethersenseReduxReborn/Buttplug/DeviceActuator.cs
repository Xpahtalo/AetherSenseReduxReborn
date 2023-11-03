using System;
using System.Security.Cryptography;
using System.Text;
using Buttplug.Core.Messages;

namespace AethersenseReduxReborn.Buttplug;

public class DeviceActuator
{
    private double _previousValue;

    public uint         Index        { get; }
    public ActuatorType ActuatorType { get; }
    public string       Description  { get; }
    public uint         Steps        { get; }
    public Device       OwnerDevice  { get; }

    public byte[] Hash        { get; }
    public string DisplayName => $"{OwnerDevice.Name} - {Index} - {ActuatorType} - {Description}";

    public DeviceActuator(Device ownerDevice, GenericDeviceMessageAttributes attributes)
    {
        Index        = attributes.Index;
        ActuatorType = attributes.ActuatorType;
        Description  = attributes.FeatureDescriptor;
        Steps        = attributes.StepCount;
        OwnerDevice  = ownerDevice;
        var hashString = $"{OwnerDevice.Name}{Index}{ActuatorType}{Description}{Steps}";
        var hash       = MD5.HashData(Encoding.UTF8.GetBytes(hashString));
        Service.PluginLog.Debug("Computed hash {0} for actuator {1}", BitConverter.ToString(hash), hashString);
        Hash = hash;
    }

    public void SendCommand(double value)
    {
        var (quantizedValue, shouldSend) = ProcessValue(value);
        if (shouldSend)
            OwnerDevice.SendCommandToActuator(Index, quantizedValue);
    }

    private (double, bool) ProcessValue(double value)
    {
        var quantized = double.Round(Steps * value) / Steps;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        // Exact comparison is intentional.
        if (quantized == _previousValue)
            return (quantized, false);
        Service.PluginLog.Debug("Quantized value {0} to {1}", value, quantized);
        _previousValue = quantized;
        return (quantized, true);
    }
}
