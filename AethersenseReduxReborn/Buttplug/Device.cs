using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AethersenseReduxReborn.Buttplug.Configs;
using Buttplug.Client;
using Buttplug.Core.Messages;

namespace AethersenseReduxReborn.Buttplug;

public class Device
{
    private ButtplugClientDevice? _internalDevice;

    public string               Name        { get; set; }
    public List<DeviceActuator> Actuators   { get; }
    public bool                 IsConnected => _internalDevice != null;

    public Device(DeviceConfig deviceConfig)
    {
        Name      = deviceConfig.Name;
        Actuators = new List<DeviceActuator>();
        foreach (var savedActuator in deviceConfig.Actuators){
            Actuators.Add(new DeviceActuator(savedActuator, this));
        }
        Service.PluginLog.Debug("Created new deviceConfig from DeviceConfig: {0}", Name);
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
        Service.PluginLog.Debug("Created new deviceConfig from ButtplugClientDevice: {0}", Name);
    }

    /// <summary>
    ///     Sends an <see cref="ActuatorCommand" /> to the internal <see cref="ButtplugClientDevice" />.
    /// </summary>
    /// <param name="actuator">The actuator of the device to send to.</param>
    /// <param name="command">The <see cref="ActuatorCommand" /> to send.</param>
    public void SendCommandToActuator(DeviceActuator actuator, ActuatorCommand command)
    {
        if (_internalDevice == null)
            return;

        // Only send the new value if it has changed enough to result in a new response from the deviceConfig. 
        Service.PluginLog.Debug("Sending value {0} to deviceConfig {1} actuator {2} - {3}", command, Name, actuator.Index, actuator.Description);
        Task.Run(async () => await _internalDevice.ScalarAsync(new ScalarCmd.ScalarSubcommand(actuator.Index, command.Value, actuator.ActuatorType)));
    }

    public void AssignInternalDevice(ButtplugClientDevice internalDevice)
    {
        if (internalDevice == null)
            throw new ArgumentNullException(nameof(internalDevice));
        if (_internalDevice != null)
            throw new ArgumentException("Internal deviceConfig already assigned.");
        if (Name != internalDevice.Name)
            throw new ArgumentException("Internal deviceConfig name does not match deviceConfig name.");
        _internalDevice = internalDevice;

        // Check that all actuators in the internal deviceConfig match actuators in the saved deviceConfig.
        foreach (var hash in from ActuatorType actuatorType in Enum.GetValues(typeof(ActuatorType))
                             from actuatorAttribute in _internalDevice.GenericAcutatorAttributes(actuatorType)
                             select ActuatorHash.FromInternalAttribute(actuatorAttribute, Name) into hash
                             where Actuators.All(actuator => actuator.Hash != hash) select hash){
            throw new ArgumentException($"Internal deviceConfig has actuator with hash {hash} that does not match any known actuator.");
        }
        Name = internalDevice.Name;
    }

    public void RemoveInternalDevice() { _internalDevice = null; }

    public DeviceConfig CreateConfig()
    {
        var actuatorConfig =
            from actuator in Actuators
            select actuator.CreateConfig();
        return new DeviceConfig {
            Name      = Name,
            Actuators = actuatorConfig.ToList(),
        };
    }
}
