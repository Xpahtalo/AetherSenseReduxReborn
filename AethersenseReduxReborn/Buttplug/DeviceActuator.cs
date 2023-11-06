using AethersenseReduxReborn.Buttplug.Configs;
using Buttplug.Core.Messages;

namespace AethersenseReduxReborn.Buttplug;

public class DeviceActuator
{
    private double _previousValue;

    public uint         Index        { get; }
    public ActuatorType ActuatorType { get; }
    public string       Description  { get; }
    public uint         Steps        { get; }
    public ActuatorHash Hash         { get; }
    public Device       OwnerDevice  { get; }
    public string       DisplayName  => $"{OwnerDevice.Name} - {Index} - {ActuatorType} - {Description}";
    public bool         IsConnected  => OwnerDevice.IsConnected;

    public DeviceActuator(DeviceActuatorConfig actuatorConfig, Device ownerDevice)
    {
        Index        = actuatorConfig.Index;
        ActuatorType = actuatorConfig.ActuatorType;
        Description  = actuatorConfig.Description;
        Steps        = actuatorConfig.Steps;
        OwnerDevice  = ownerDevice;
        Hash         = actuatorConfig.Hash;
        Service.PluginLog.Debug("Created known actuator from config: {0} with hash {1}", DisplayName, Hash);
    }

    public DeviceActuator(Device ownerDevice, GenericDeviceMessageAttributes attributes)
    {
        Index        = attributes.Index;
        ActuatorType = attributes.ActuatorType;
        Description  = attributes.FeatureDescriptor;
        Steps        = attributes.StepCount;
        OwnerDevice  = ownerDevice;
        Hash         = new ActuatorHash(this);
        Service.PluginLog.Debug("Created new actuator from ButtplugClientDevice: {0} with hash {1}", DisplayName, Hash);
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
        Service.PluginLog.Verbose("Quantized value {0} to {1}", value, quantized);
        _previousValue = quantized;
        return (quantized, true);
    }

    public DeviceActuatorConfig CreateConfig() =>
        new() {
            Index        = Index,
            ActuatorType = ActuatorType,
            Description  = Description,
            Steps        = Steps,
            Hash         = Hash,
        };
}
