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

    public IEnumerable<Device> Devices => from device in _devices
                                          select device;

    public IEnumerable<Device> ConnectedDevices => from device in _devices
                                                   where device.IsConnected
                                                   select device;

    public IEnumerable<DeviceActuator> Actuators => from device in _devices
                                                    from actuator in device.Actuators
                                                    select actuator;

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
        var deviceName     = buttplugDevice.Name;
        var existingDevice = Devices.FirstOrDefault(device => device.Name == deviceName);
        var actuatorHashes = from attribute in buttplugDevice.GetGenericDeviceMessageAttributes()
                             select new ActuatorHash(attribute, deviceName);
        var conflictingHashes = Actuators.Select(actuator => actuator.Hash).Intersect(actuatorHashes);
        var actuatorConflict  = conflictingHashes.Any();

        if (actuatorConflict){
            Service.PluginLog.Error("Tried to add new buttplug device, but found conflicting actuator hashes. {0}", conflictingHashes);
            return;
        }
        if (existingDevice is not null){
            existingDevice.AssignInternalDevice(buttplugDevice);
            InvokeActuatorConnectedOnDevice(existingDevice);
        } else{
            var newDevice = new Device(buttplugDevice);
            _devices.Add(newDevice);
            InvokeActuatorConnectedOnDevice(newDevice);
        }
    }

    public void DisconnectButtplugDevice(ButtplugClientDevice clientDevice)
    {
        try{
            var knownDevice = Devices.First(device => device.Name == clientDevice.Name);
            DisconnectDevice(knownDevice);
        } catch (InvalidOperationException e){
            Service.PluginLog.Error(e, "Found more than one deviceConfig with name {0}", clientDevice.Name);
        } catch (KeyNotFoundException e){
            Service.PluginLog.Error(e, "Found no deviceConfig with name {0}", clientDevice.Name);
        }
    }

    public void DisconnectAllDevices()
    {
        foreach (var device in Devices){
            DisconnectDevice(device);
        }
    }

    public void DisconnectDevice(Device device)
    {
        device.RemoveInternalDevice();
        InvokeActuatorDisconnectedOnDevice(device);
    }

    private void InvokeActuatorConnectedOnDevice(Device device)
    {
        foreach (var actuator in device.Actuators){
            InvokeActuatorConnected(actuator);
        }
    }

    private void InvokeActuatorConnected(DeviceActuator actuator)
    {
        ActuatorConnected?.Invoke(new ActuatorConnectedEventArgs {
            HashOfActuator = actuator.Hash,
        });
    }

    private void InvokeActuatorDisconnectedOnDevice(Device device)
    {
        foreach (var actuator in device.Actuators){
            InvokeActuatorDisconnected(actuator);
        }
    }

    private void InvokeActuatorDisconnected(DeviceActuator actuator)
    {
        ActuatorDisconnected?.Invoke(new ActuatorDisconnectedEventArgs {
            HashOfActuator = actuator.Hash,
        });
    }

    public DeviceActuator? GetActuatorByHash(ActuatorHash hash)
    {
        var enumerable = Actuators;
        // Guard against empty list
        if (!enumerable.Any())
            return null;
        return Actuators.FirstOrDefault(actuator => actuator.Hash == hash);
    }
}
