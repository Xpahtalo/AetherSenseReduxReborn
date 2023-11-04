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
    private readonly DeviceCollection            _deviceCollection;

    public bool                          Connected          => _buttplugClient.Connected;
    public IReadOnlyList<Device>         Devices            => _deviceCollection.KnownDevices;
    public IReadOnlyList<Device>         ConnectedDevices   => Devices.Where(device => device.IsConnected).ToList().AsReadOnly();
    public IReadOnlyList<DeviceActuator> Actuators          => Devices.SelectMany(device => device.Actuators).ToList().AsReadOnly();
    public IEnumerable<DeviceActuator>   ConnectedActuators => ConnectedDevices.SelectMany(device => device.Actuators).ToList().AsReadOnly();

    public delegate void                 ServerConnectedHandler(object? sender, EventArgs args);
    public event ServerConnectedHandler? ServerConnectedEvent;

    /// <summary>
    ///     Called when an <see cref="DeviceActuator" /> has been connected to the server.
    /// </summary>
    public event DeviceCollection.ActuatorConnectedEventHandler? ActuatorConnected {
        add => _deviceCollection.ActuatorConnected += value;
        remove => _deviceCollection.ActuatorConnected -= value;
    }
    /// <summary>
    ///     Called when an <see cref="DeviceActuator" /> has been disconnected from the server.
    /// </summary>
    public event DeviceCollection.ActuatorDisconnectedEventHandler? ActuatorDisconnected {
        add => _deviceCollection.ActuatorDisconnected += value;
        remove => _deviceCollection.ActuatorDisconnected -= value;
    }

    public ButtplugWrapper(string name, ButtplugPluginConfiguration pluginConfiguration)
    {
        _pluginConfiguration          =  pluginConfiguration;
        _deviceCollection             =  new DeviceCollection(pluginConfiguration);
        _buttplugClient               =  new ButtplugClient(name);
        _buttplugClient.DeviceAdded   += DeviceAdded;
        _buttplugClient.DeviceRemoved += DeviceRemoved;
    }

    public void SendCommandToActuator(ActuatorHash hash, double value)
    {
        try{
            var actuator = GetActuatorByHash(hash);
            if (actuator is null)
                throw new KeyNotFoundException();
            if (!actuator.IsConnected)
                throw new InvalidOperationException("Actuator is not connected.");
            actuator.SendCommand(value);
        } catch (KeyNotFoundException ex){
            Service.PluginLog.Error(ex, "Could not locate actuator with hash {0}", hash);
        }
    }

    public bool IsActuatorConnected(ActuatorHash hash)
    {
        var actuator = GetActuatorByHash(hash);
        return actuator is not null
            && actuator.IsConnected;
    }

    public string GetActuatorDisplayName(ActuatorHash hash)
    {
        var actuator = GetActuatorByHash(hash);
        return actuator is null
                   ? "Unknown Actuator"
                   : actuator.DisplayName;
    }

    public DeviceActuator? GetActuatorByHash(ActuatorHash hash) => Actuators.SingleOrDefault(actuator => actuator.Hash == hash);

    public void SaveDevicesToConfiguration()
    {
        Service.PluginLog.Information("Saving devices to configuration.");
        _pluginConfiguration.SavedDevices.Clear();
        foreach (var device in Devices){
            _pluginConfiguration.SavedDevices.Add(new SavedDevice(device));
        }
        Service.ConfigurationService.SaveServerConfiguration(_pluginConfiguration);
    }

#region ButtplugClient passthrough

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
        // DeviceRemoved event does not get called when the server is disconnected, so we need to manually remove all devices.
        _deviceCollection.DisconnectAllDevices();
    }

    private void DeviceAdded(object? sender, DeviceAddedEventArgs args)
    {
        try{
            Service.PluginLog.Information("New ButtplugDevice connected: {0}", args.Device.Name);
            _deviceCollection.AddNewButtplugDevice(args.Device);
        } catch (Exception){
            Service.PluginLog.Warning("Unable to add device to list of devices");
        }
    }

    private void DeviceRemoved(object? sender, DeviceRemovedEventArgs args)
    {
        try{
            _deviceCollection.DisconnectButtplugDevice(args.Device);
        } catch (Exception ex){
            Service.PluginLog.Error(ex, "Unable to remove device from list of devices");
        }
    }

#endregion

    public void Dispose()
    {
        _buttplugClient.DisconnectAsync();
        _buttplugClient.Dispose();
        _buttplugCts?.Dispose();
    }
}
