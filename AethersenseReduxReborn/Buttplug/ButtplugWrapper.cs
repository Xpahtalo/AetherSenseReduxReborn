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

    public bool                        Connected          => _buttplugClient.Connected;
    public IEnumerable<Device>         Devices            => _deviceCollection.KnownDevices;
    public IEnumerable<Device>         ConnectedDevices   => _deviceCollection.ConnectedDevices;
    public IEnumerable<DeviceActuator> Actuators          => _deviceCollection.Actuators;
    public IEnumerable<DeviceActuator> ConnectedActuators => _deviceCollection.ConnectedActuators;

    public delegate void                 ServerConnectedHandler(object? sender, EventArgs args);
    public event ServerConnectedHandler? ServerConnectedEvent;

    public event ActuatorConnectedEventHandler? ActuatorConnected {
        add => _deviceCollection.ActuatorConnected += value;
        remove => _deviceCollection.ActuatorConnected -= value;
    }
    public event ActuatorDisconnectedEventHandler? ActuatorDisconnected {
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

    /// <summary>
    ///     Send a command to an <see cref="DeviceActuator">actuator</see>, identified by its
    ///     <see cref="ActuatorHash">hash</see>.
    /// </summary>
    /// <param name="hash">
    ///     The <see cref="ActuatorHash">hash</see> of the <see cref="DeviceActuator">actuator</see> to send the
    ///     command to.
    /// </param>
    /// <param name="command">
    ///     The <see cref="ActuatorCommand" /> to send to the <see cref="DeviceActuator">actuator</see>
    /// </param>
    /// <exception cref="KeyNotFoundException">The <see cref="DeviceActuator">actuator</see> is not found.</exception>
    /// <exception cref="InvalidOperationException">
    ///     The <see cref="DeviceActuator">actuator</see> is found, but it is not
    ///     connected so it cannot be sent commands.
    /// </exception>
    public void SendCommandToActuator(ActuatorHash hash, ActuatorCommand command)
    {
        try{
            var actuator = GetActuatorByHash(hash);
            if (actuator is null)
                throw new KeyNotFoundException();

            if (!actuator.IsConnected)
                throw new InvalidOperationException("Actuator is not connected.");

            actuator.SendCommand(command);
        } catch (KeyNotFoundException e){
            Service.PluginLog.Error(e, "Could not locate actuator with hash {0}", hash);
            throw;
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

    /// <inheritdoc cref="DeviceCollection.GetActuatorByHash" />
    public DeviceActuator? GetActuatorByHash(ActuatorHash hash) => _deviceCollection.GetActuatorByHash(hash);

    public void SaveDevicesToConfiguration()
    {
        Service.PluginLog.Information("Saving devices to configuration.");
        _pluginConfiguration.SavedDevices.Clear();
        var deviceConfigs =
            from device in Devices
            select device.CreateConfig();
        _pluginConfiguration.SavedDevices.AddRange(deviceConfigs);
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
                     } catch (Exception e){
                         Service.PluginLog.Error(e, "Failed to connect to buttplug server.");
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
        } catch (Exception e){
            Service.PluginLog.Error(e, "Unable to remove device from list of devices");
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
