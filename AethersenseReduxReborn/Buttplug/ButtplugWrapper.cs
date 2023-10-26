using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AethersenseReduxReborn.Configurations;
using Buttplug.Client;
using Buttplug.Client.Connectors.WebsocketConnector;

namespace AethersenseReduxReborn.Buttplug;

public sealed class ButtplugWrapper: IDisposable
{
    private CancellationTokenSource? _buttplugCts;

    private readonly ButtplugPluginConfiguration _pluginConfiguration;
    private readonly ButtplugClient              _buttplugClient;

    public bool                               Connected => _buttplugClient.Connected;
    public List<Device>                       Devices   { get; }
    public Dictionary<byte[], DeviceActuator> Actuators { get; } = new();

    public delegate void                    ActuatorAddedEventHandler(object? sender, ActuatorAddedEventArgs args);
    public event ActuatorAddedEventHandler? ActuatorAddedEvent;

    public delegate void                      ActuatorRemovedEventHandler(object? sender, ActuatorRemovedEventArgs args);
    public event ActuatorRemovedEventHandler? ActuatorRemovedEvent;

    public delegate void                 ServerConnectedHandler(object? sender, EventArgs args);
    public event ServerConnectedHandler? ServerConnectedEvent;

    public ButtplugWrapper(string name, ButtplugPluginConfiguration pluginConfiguration)
    {
        _pluginConfiguration          =  pluginConfiguration;
        _buttplugClient               =  new ButtplugClient(name);
        Devices                       =  new List<Device>();
        _buttplugClient.DeviceAdded   += DeviceAdded;
        _buttplugClient.DeviceRemoved += DeviceRemoved;
    }

    public void SendCommandToActuator(byte[] hash, double value)
    {
        try{
            Actuators[hash].SendCommand(value);
        } catch (KeyNotFoundException e){
            Service.PluginLog.Error("Could not locate actuator with hash {0}", hash);
            throw;
        }
    }

    public void Connect()
    {
        Task.Run(async () =>
                 {
                     _buttplugCts = new CancellationTokenSource();
                     try{
                         await _buttplugClient.ConnectAsync(new ButtplugWebsocketConnector(new Uri(_pluginConfiguration.Address)),
                                                            _buttplugCts.Token);
                         Service.PluginLog.Information("Connected to server.");
                         ServerConnectedEvent?.Invoke(this, EventArgs.Empty);
                     } catch (Exception ex){
                         Service.PluginLog.Error(ex, "Failed to connect to buttplug server.");
                     }
                 });
    }

    public void Disconnect()
    {
        Task.Run(async () =>
                 {
                     await _buttplugClient.DisconnectAsync();
                     _buttplugCts?.Cancel();
                 });
    }

    private void DeviceAdded(object? sender, DeviceAddedEventArgs args)
    {
        Service.PluginLog.Information("Adding device: {0}", args.Device.Name);
        var newDevice = new Device(args.Device);

        foreach (var actuator in newDevice.Actuators){
            Service.PluginLog.Information("Adding actuator: {0} with hash {1}", actuator.DisplayName, actuator.Hash);
            Actuators.Add(actuator.Hash, actuator);
            ActuatorAddedEvent?.Invoke(this,
                                       new ActuatorAddedEventArgs {
                                           HashOfActuator = actuator.Hash,
                                       });
        }

        Devices.Add(newDevice);
    }

    private void DeviceRemoved(object? sender, DeviceRemovedEventArgs args)
    {
        try{
            Service.PluginLog.Information("Removing device: {0}", args.Device.Name);
            foreach (var actuator in Devices.Where(device => device.Name == args.Device.Name).SelectMany(device => device.Actuators)){
                RemoveActuator(actuator.Hash);
            }
            Devices.Remove(Devices.Single(d => d.Name == args.Device.Name));
        } catch (Exception e){
            Service.PluginLog.Error("Unable to remove device from list of devices", e);
        }
    }

    private void RemoveActuator(byte[]? hash)
    {
        if (hash is null)
            return;
        if (!Actuators.ContainsKey(hash))
            return;
        Service.PluginLog.Information("Removing actuator: {0} with hash {1}", Actuators[hash].DisplayName, hash);
        Actuators.Remove(hash);
        ActuatorRemovedEvent?.Invoke(this,
                                     new ActuatorRemovedEventArgs {
                                         HashOfActuator = hash,
                                     });
    }

    public void Dispose()
    {
        _buttplugClient.DisconnectAsync();
        _buttplugClient.Dispose();
        _buttplugCts?.Dispose();
    }
}

public class ActuatorRemovedEventArgs: EventArgs
{
    public required byte[] HashOfActuator { get; init; }
}

public class ActuatorAddedEventArgs: EventArgs
{
    public required byte[] HashOfActuator { get; init; }
}
