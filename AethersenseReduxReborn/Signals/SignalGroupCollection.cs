using System;
using System.Collections.Generic;
using System.Linq;
using AethersenseReduxReborn.Buttplug;
using AethersenseReduxReborn.Buttplug.CustomEventArgs;
using AethersenseReduxReborn.Signals.Configs;

namespace AethersenseReduxReborn.Signals;

public sealed class SignalGroupCollection: IDisposable
{
    private List<SignalGroup> SignalGroups { get; } = new();

    private ButtplugWrapper ButtplugWrapper { get; }

    public SignalGroupCollection(IEnumerable<SignalGroupConfiguration> groupConfigurations, ButtplugWrapper buttplugWrapper)
    {
        ButtplugWrapper = buttplugWrapper;

        foreach (var groupConfiguration in groupConfigurations){
            AddSignalGroup(groupConfiguration);
        }

        ButtplugWrapper.ActuatorConnected    += ActuatorConnected;
        ButtplugWrapper.ActuatorDisconnected += ActuatorDisconnected;
    }

    private void AddSignalGroup(SignalGroupConfiguration groupConfiguration)
    {
        Service.PluginLog.Debug("Applying new Signal Group. Name: {0}, CombineType: {1}, HashOfLastAssignedActuator: {2}", groupConfiguration.Name, groupConfiguration.CombineType, groupConfiguration.HashOfLastAssignedActuator);
        var signalGroup = new SignalGroup(groupConfiguration);
        SignalGroups.Add(signalGroup);

        if (signalGroup.HashOfLastAssignedActuator != ActuatorHash.Zeroed
         && ButtplugWrapper.IsActuatorConnected(signalGroup.HashOfLastAssignedActuator)){
            signalGroup.Enable();
        }
    }

    /// <summary>
    ///     Update all <see cref="SignalGroup">signal groups</see> and send commands to their assign
    ///     <see cref="DeviceActuator">actuators</see>.
    /// </summary>
    /// <param name="frameworkDelta">The number of milliseconds since the last update.</param>
    public void UpdateSignalGroups(double frameworkDelta)
    {
        if (ButtplugWrapper.Connected == false){
            return;
        }

        var enabledSignalGroups =
            from signalGroup in SignalGroups
            where signalGroup.Enabled
            where signalGroup.HashOfAssignedActuator != ActuatorHash.Zeroed
            select signalGroup;

        foreach (var signalGroup in enabledSignalGroups){
            try{
                signalGroup.UpdateSources(frameworkDelta);
                ButtplugWrapper.SendCommandToActuator(signalGroup.HashOfAssignedActuator, new ActuatorCommand(signalGroup.Signal));
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

        foreach (var signalGroup in SignalGroups.Where(signalGroup => signalGroup.Enabled)){
            signalGroup.UpdateSources(frameworkDelta);
        }
    }

    public IEnumerable<SignalGroupConfiguration> CreateConfig() =>
        from signalGroup in SignalGroups
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
            from signalGroup in SignalGroups
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
            from signalGroup in SignalGroups
            where signalGroup.HashOfAssignedActuator == args.HashOfActuator
            select signalGroup;
        foreach (var group in groupsToDisable){
            group.Disable();
        }
    }

    private void Clear()
    {
        foreach (var signalGroup in SignalGroups){
            signalGroup.Dispose();
        }
        SignalGroups.Clear();
    }

    public void Dispose()
    {
        ButtplugWrapper.ActuatorConnected    -= ActuatorConnected;
        ButtplugWrapper.ActuatorDisconnected -= ActuatorDisconnected;
        Clear();
    }
}
