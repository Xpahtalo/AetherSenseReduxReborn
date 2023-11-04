using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Buttplug.Client;
using Buttplug.Core.Messages;

namespace AethersenseReduxReborn.Buttplug;

public class Device
{
    private ButtplugClientDevice? _internalDevice;

    public string               Name        { get; set; }
    public List<DeviceActuator> Actuators   { get; }
    public bool                 IsConnected => _internalDevice != null;

    public Device(SavedDevice savedDevice)
    {
        Name      = savedDevice.Name;
        Actuators = new List<DeviceActuator>();
        foreach (var savedActuator in savedDevice.Actuators){
            Actuators.Add(new DeviceActuator(savedActuator, this));
        }
        Service.PluginLog.Debug("Created new device from SavedDevice: {0}", Name);
    }

    public Device(ButtplugClientDevice internalDevice)
    {
        _internalDevice = internalDevice;
        Name            = internalDevice.Name;
        var internalList = new List<GenericDeviceMessageAttributes>();
        foreach (ActuatorType actuatorType in Enum.GetValues(typeof(ActuatorType))){
            internalList.AddRange(_internalDevice.GenericAcutatorAttributes(actuatorType));
        }

        Actuators = new List<DeviceActuator>();
        foreach (var actuator in internalList){
            Actuators.Add(new DeviceActuator(this, actuator));
        }
        Service.PluginLog.Debug("Created new device from ButtplugClientDevice: {0}", Name);
    }

    public void SendCommandToActuator(uint index, double value)
    {
        if (_internalDevice == null)
            return;

        var actuator = Actuators.Single(actuator => actuator.Index == index);

        // Only send the new value if it has changed enough to result in a new response from the device. 
        Service.PluginLog.Debug("Sending value {0} to device {1} actuator {2}", value, Name, actuator.Description);
        Task.Run(async () => await _internalDevice.ScalarAsync(new ScalarCmd.ScalarSubcommand(actuator.Index, value, actuator.ActuatorType)));
    }

    public void AssignInternalDevice(ButtplugClientDevice internalDevice)
    {
        if (internalDevice == null)
            throw new ArgumentNullException(nameof(internalDevice));
        if (_internalDevice != null)
            throw new ArgumentException("Internal device already assigned.");
        if (Name != internalDevice.Name)
            throw new ArgumentException("Internal device name does not match device name.");
        _internalDevice = internalDevice;

        // Check that all actuators in the internal device match actuators in the saved device.
        foreach (var hash in from ActuatorType actuatorType in Enum.GetValues(typeof(ActuatorType))
                             from actuatorAttribute in _internalDevice.GenericAcutatorAttributes(actuatorType)
                             select ActuatorHash.FromInternalAttribute(actuatorAttribute, Name) into hash
                             where Actuators.All(actuator => actuator.Hash != hash) select hash){
            throw new ArgumentException($"Internal device has actuator with hash {hash} that does not match any known actuator.");
        }
        Name = internalDevice.Name;
    }

    public void RemoveInternalDevice() { _internalDevice = null; }
}

public class SavedDevice
{
    public string                    Name      { get; set; }
    public List<SavedDeviceActuator> Actuators { get; set; }

    [JsonConstructor]
    public SavedDevice(string name, List<SavedDeviceActuator> actuators)
    {
        Name      = name;
        Actuators = actuators;
    }

    public SavedDevice(Device device)
    {
        Service.PluginLog.Debug("Saving device {0} to configuration.", device.Name);
        Name      = device.Name;
        Actuators = new List<SavedDeviceActuator>();
        foreach (var actuator in device.Actuators){
            Actuators.Add(new SavedDeviceActuator(actuator));
        }
    }
}
