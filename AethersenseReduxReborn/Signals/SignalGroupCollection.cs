using System;
using System.Collections.Generic;
using System.Linq;
using AethersenseReduxReborn.Buttplug;
using AethersenseReduxReborn.Buttplug.CustomEventArgs;
using AethersenseReduxReborn.Signals.Configs;

namespace AethersenseReduxReborn.Signals;

public sealed class SignalGroupCollection: IDisposable
{
    private readonly List<SignalGroup> _signalGroups = new();

    private readonly ButtplugWrapper _buttplugWrapper;

    public SignalGroupCollection(IEnumerable<SignalGroupConfiguration> groupConfigurations, ButtplugWrapper buttplugWrapper)
    {
        _buttplugWrapper = buttplugWrapper;

        foreach (var groupConfiguration in groupConfigurations){
            AddSignalGroup(groupConfiguration);
        }

        _buttplugWrapper.ActuatorConnected    += ActuatorConnected;
        _buttplugWrapper.ActuatorDisconnected += ActuatorDisconnected;
    }

    private void AddSignalGroup(SignalGroupConfiguration groupConfiguration)
    {
        Service.PluginLog.Debug("Applying new Signal Group. Name: {0}, CombineType: {1}, HashOfLastAssignedActuator: {2}", groupConfiguration.Name, groupConfiguration.CombineType, groupConfiguration.HashOfLastAssignedActuator);
        var signalGroup = new SignalGroup(groupConfiguration);
        _signalGroups.Add(signalGroup);

        if (signalGroup.HashOfLastAssignedActuator != ActuatorHash.Zeroed
         && _buttplugWrapper.IsActuatorConnected(signalGroup.HashOfLastAssignedActuator))
            signalGroup.Enable();
    }

    /// <summary>
    ///     Update all <see cref="SignalGroup">signal groups</see> and send commands to their assign
    ///     <see cref="DeviceActuator">actuators</see>.
    /// </summary>
    /// <param name="frameworkDelta">The number of milliseconds since the last update.</param>
    public void UpdateSignalGroups(double frameworkDelta)
    {
        if (_buttplugWrapper.Connected == false)
            return;

        var enabledSignalGroups =
            from signalGroup in _signalGroups
            where signalGroup.Enabled
            where signalGroup.HashOfAssignedActuator != ActuatorHash.Zeroed
            select signalGroup;

        foreach (var signalGroup in enabledSignalGroups){
            try{
                signalGroup.UpdateSources(frameworkDelta);
                _buttplugWrapper.SendCommandToActuator(signalGroup.HashOfAssignedActuator, new ActuatorCommand(signalGroup.Signal));
            } catch (Exception e){
                switch (e){
                    case KeyNotFoundException:
                    case InvalidOperationException:
                        Service.PluginLog.Warning("Unable to send command to actuator. ActuatorHash: {0}", signalGroup.HashOfAssignedActuator);
                        signalGroup.Disable();
                        break;
                    default:
                        throw;
                }
            }
        }

        foreach (var signalGroup in _signalGroups.Where(signalGroup => signalGroup.Enabled)){
            signalGroup.UpdateSources(frameworkDelta);
        }
    }

    public IEnumerable<SignalGroupConfiguration> CreateConfig() =>
        from signalGroup in _signalGroups
        select signalGroup.CreateConfiguration();

    /// <summary>
    ///     Scan the list of <see cref="SignalGroup">signal groups</see> for any that have the
    ///     <see cref="ActuatorHash">hash</see> of the newly connected <see cref="DeviceActuator">actuator</see> and enable
    ///     them.
    /// </summary>
    private void ActuatorConnected(ActuatorConnectedEventArgs args)
    {
        Service.PluginLog.Debug("Actuator with hash {0} connected. Scanning for SignalGroups to enable.", args.HashOfActuator);
        var groupsToEnable =
            from signalGroup in _signalGroups
            where signalGroup.HashOfLastAssignedActuator == args.HashOfActuator
            select signalGroup;
        foreach (var group in groupsToEnable){
            group.Enable();
        }
    }

    /// <summary>
    ///     Scan the list of <see cref="SignalGroup">signal groups</see> for any that have the
    ///     <see cref="ActuatorHash">hash</see> of the newly disconnected <see cref="DeviceActuator">actuator</see> and disable
    ///     them.
    /// </summary>
    private void ActuatorDisconnected(ActuatorDisconnectedEventArgs args)
    {
        Service.PluginLog.Debug("Actuator with hash {0} disconnected. Scanning for SignalGroups to disable.", args.HashOfActuator);
        var groupsToDisable =
            from signalGroup in _signalGroups
            where signalGroup.HashOfAssignedActuator == args.HashOfActuator
            select signalGroup;
        foreach (var group in groupsToDisable){
            group.Disable();
        }
    }

    private void Clear()
    {
        foreach (var signalGroup in _signalGroups){
            signalGroup.Dispose();
        }
        _signalGroups.Clear();
    }

    public void Dispose()
    {
        _buttplugWrapper.ActuatorConnected    -= ActuatorConnected;
        _buttplugWrapper.ActuatorDisconnected -= ActuatorDisconnected;
        Clear();
    }
}
