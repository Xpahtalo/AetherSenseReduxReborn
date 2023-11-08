using AethersenseReduxReborn.Buttplug.Configs;
using Buttplug.Core.Messages;

namespace AethersenseReduxReborn.Buttplug;

public class DeviceActuator
{
    private ActuatorCommand _previousValue;

    public uint         Index             { get; }
    public ActuatorType ActuatorType      { get; }
    public string       Description       { get; }
    public uint         Steps             { get; }
    public ActuatorHash Hash              { get; }
    public Device       OwnerDevice       { get; }
    public string       DisplayAttributes => $"{Index} - {ActuatorType} - {Description}";
    public string       DisplayName       => $"{OwnerDevice.Name} - {DisplayAttributes}";
    public bool         IsConnected       => OwnerDevice.IsConnected;

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

    public void SendCommand(ActuatorCommand value)
    {
        var quantized = value.Quantized(Steps);

        if (quantized == _previousValue)
            return;
        Service.PluginLog.Verbose("New actuator command [{0}] is significantly different from previous command [{1}]. Sending.", quantized, _previousValue);
        _previousValue = quantized;
        OwnerDevice.SendCommandToActuator(this, quantized);
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
