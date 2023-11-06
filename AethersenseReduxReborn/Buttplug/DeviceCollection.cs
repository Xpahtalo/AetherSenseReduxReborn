using System;
using System.Collections.Generic;
using System.Linq;
using AethersenseReduxReborn.Buttplug.CustomEventArgs;
using AethersenseReduxReborn.Configurations;
using Buttplug.Client;

namespace AethersenseReduxReborn.Buttplug;

public class DeviceCollection
{
    private readonly HashSet<Device> _devices = new();

    public IReadOnlyList<Device> KnownDevices => _devices.ToList().AsReadOnly();

    public delegate void                        ActuatorConnectedEventHandler(ActuatorConnectedEventArgs args);
    public event ActuatorConnectedEventHandler? ActuatorConnected;

    public delegate void                           ActuatorDisconnectedEventHandler(ActuatorDisconnectedEventArgs args);
    public event ActuatorDisconnectedEventHandler? ActuatorDisconnected;

    public DeviceCollection(ButtplugPluginConfiguration configuration)
    {
        Service.PluginLog.Information("Loading saved devices from config.");
        foreach (var device in configuration.SavedDevices){
            AddSavedDevice(device);
        }
    }

    private void AddSavedDevice(SavedDevice device)
    {
        Service.PluginLog.Debug("Adding saved device from config: {0}", device.Name);
        var newDevice = new Device(device);
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
                // Create and add a new device
                var newDevice = new Device(buttplugDevice);
                _devices.Add(newDevice);
                InvokeActuatorConnected(newDevice);
            } else{
                // Otherwise, try to assign to an existing device
                try{
                    knownDevice.AssignInternalDevice(buttplugDevice);
                    InvokeActuatorConnected(knownDevice);
                } catch (ArgumentException ex){
                    Service.PluginLog.Error(ex, "Failed to assign internal device to known device.");
                }
            }
        } catch (InvalidOperationException ex){
            Service.PluginLog.Error(ex, "Found more than one device with name {0}", buttplugDevice.Name);
        } catch (Exception ex){
            Service.PluginLog.Error(ex, "Failed to add new device.");
        }
    }

    public void DisconnectButtplugDevice(ButtplugClientDevice clientDevice)
    {
        try{
            var knownDevice = _devices.Single(device => device.Name == clientDevice.Name);
            DisconnectDevice(knownDevice);
        } catch (InvalidOperationException ex){
            Service.PluginLog.Error(ex, "Found more than one device with name {0}", clientDevice.Name);
        } catch (KeyNotFoundException ex){
            Service.PluginLog.Error(ex, "Found no device with name {0}", clientDevice.Name);
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
}
