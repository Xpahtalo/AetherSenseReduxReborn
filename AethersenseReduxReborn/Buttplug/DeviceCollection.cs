using System;
using System.Collections.Generic;
using System.Linq;
using AethersenseReduxReborn.Buttplug.Configs;
using AethersenseReduxReborn.Buttplug.CustomEventArgs;
using AethersenseReduxReborn.Configurations;
using Buttplug.Client;

namespace AethersenseReduxReborn.Buttplug;

public class DeviceCollection
{
    private readonly HashSet<Device> _devices = new();

    /// <summary>
    ///     All <see cref="Device">devices</see> in the collection.
    /// </summary>
    public IEnumerable<Device> KnownDevices => from device in _devices
                                               select device;

    /// <summary>
    ///     Only <see cref="Device">devices</see> that are currently connected.
    /// </summary>
    public IEnumerable<Device> ConnectedDevices => from device in _devices
                                                   where device.IsConnected
                                                   select device;

    /// <summary>
    ///     All actuators of all <see cref="Device">devices</see> in the collection.
    /// </summary>
    public IEnumerable<DeviceActuator> Actuators => from device in _devices
                                                    from actuator in device.Actuators
                                                    select actuator;

    /// <summary>
    ///     Only <see cref="DeviceActuator">actuators</see> of <see cref="Device">devices</see> that are currently connected.
    /// </summary>
    public IEnumerable<DeviceActuator> ConnectedActuators => from device in _devices
                                                             where device.IsConnected
                                                             from actuator in device.Actuators
                                                             select actuator;

    public event ActuatorConnectedEventHandler?    ActuatorConnected;
    public event ActuatorDisconnectedEventHandler? ActuatorDisconnected;

    public DeviceCollection(ButtplugPluginConfiguration configuration)
    {
        Service.PluginLog.Information("Loading saved devices from config.");
        foreach (var device in configuration.SavedDevices){
            AddSavedDevice(device);
        }
    }

    private void AddSavedDevice(DeviceConfig deviceConfig)
    {
        Service.PluginLog.Debug("Adding saved deviceConfig from config: {0}", deviceConfig.Name);
        var newDevice = new Device(deviceConfig);
        _devices.Add(newDevice);
        foreach (var actuator in newDevice.Actuators){
            Service.PluginLog.Debug("Adding known actuator from config: {0} with hash {1}", actuator.DisplayName, actuator.Hash);
        }
    }

    public void AddNewButtplugDevice(ButtplugClientDevice buttplugDevice)
    {
        try{
            var knownDevice = _devices.SingleOrDefault(device => device.Name == buttplugDevice.Name);
            // If it doesn't exist
            if (knownDevice is null){
                // Create and add a new deviceConfig
                var newDevice = new Device(buttplugDevice);
                _devices.Add(newDevice);
                InvokeActuatorConnected(newDevice);
            } else{
                // Otherwise, try to assign to an existing deviceConfig
                try{
                    knownDevice.AssignInternalDevice(buttplugDevice);
                    InvokeActuatorConnected(knownDevice);
                } catch (ArgumentException e){
                    Service.PluginLog.Error(e, "Failed to assign internal deviceConfig to known deviceConfig.");
                }
            }
        } catch (InvalidOperationException e){
            Service.PluginLog.Error(e, "Found more than one deviceConfig with name {0}", buttplugDevice.Name);
        } catch (Exception e){
            Service.PluginLog.Error(e, "Failed to add new deviceConfig.");
        }
    }

    public void DisconnectButtplugDevice(ButtplugClientDevice clientDevice)
    {
        try{
            var knownDevice = _devices.Single(device => device.Name == clientDevice.Name);
            DisconnectDevice(knownDevice);
        } catch (InvalidOperationException e){
            Service.PluginLog.Error(e, "Found more than one deviceConfig with name {0}", clientDevice.Name);
        } catch (KeyNotFoundException e){
            Service.PluginLog.Error(e, "Found no deviceConfig with name {0}", clientDevice.Name);
        }
    }

    public void DisconnectAllDevices()
    {
        foreach (var device in _devices){
            DisconnectDevice(device);
        }
    }

    public void DisconnectDevice(Device device)
    {
        device.RemoveInternalDevice();
        InvokeActuatorDisconnected(device);
    }

    private void InvokeActuatorConnected(Device device)
    {
        foreach (var actuator in device.Actuators){
            ActuatorConnected?.Invoke(new ActuatorConnectedEventArgs {
                HashOfActuator = actuator.Hash,
            });
        }
    }

    private void InvokeActuatorDisconnected(Device device)
    {
        foreach (var actuator in device.Actuators){
            ActuatorDisconnected?.Invoke(new ActuatorDisconnectedEventArgs {
                HashOfActuator = actuator.Hash,
            });
        }
    }

    /// <summary>
    ///     Returns the <see cref="DeviceActuator" /> with the given <see cref="ActuatorHash" />.
    /// </summary>
    /// <param name="hash">
    ///     The <see cref="ActuatorHash">hash</see> of the <see cref="DeviceActuator">actuator</see> to search
    ///     for.
    /// </param>
    /// <returns>The matching <see cref="DeviceActuator">actuator</see>, otherwise null.</returns>
    public DeviceActuator? GetActuatorByHash(ActuatorHash hash)
    {
        var enumerable = Actuators;
        // Guard against empty list
        if (!enumerable.Any())
            return null;
        return Actuators.FirstOrDefault(actuator => actuator.Hash == hash);
    }
}
