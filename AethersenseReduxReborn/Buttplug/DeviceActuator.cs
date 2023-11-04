using System.Text.Json.Serialization;
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

    public DeviceActuator(SavedDeviceActuator savedActuator, Device ownerDevice)
    {
        Index        = savedActuator.Index;
        ActuatorType = savedActuator.ActuatorType;
        Description  = savedActuator.Description;
        Steps        = savedActuator.Steps;
        OwnerDevice  = ownerDevice;
        Hash         = savedActuator.Hash;
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
        Service.PluginLog.Debug("Quantized value {0} to {1}", value, quantized);
        _previousValue = quantized;
        return (quantized, true);
    }
}

public class SavedDeviceActuator
{
    public uint         Index        { get; set; }
    public ActuatorType ActuatorType { get; set; }
    public string       Description  { get; set; }
    public uint         Steps        { get; set; }
    public ActuatorHash Hash         { get; set; }
    [JsonIgnore]
    public string DisplayName => $"{Index} - {ActuatorType} - {Description}";

    [JsonConstructor]
    public SavedDeviceActuator(uint index, ActuatorType actuatorType, string description, uint steps, ActuatorHash hash)
    {
        Index        = index;
        ActuatorType = actuatorType;
        Description  = description;
        Steps        = steps;
        Hash         = hash;
    }

    public SavedDeviceActuator(DeviceActuator actuator)
    {
        Index        = actuator.Index;
        ActuatorType = actuator.ActuatorType;
        Description  = actuator.Description;
        Steps        = actuator.Steps;
        Hash         = actuator.Hash;
        Service.PluginLog.Debug("Created saved actuator from actuator: {0}", DisplayName);
    }
}
