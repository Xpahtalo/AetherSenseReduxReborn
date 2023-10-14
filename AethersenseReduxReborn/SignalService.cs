using System;
using System.Collections.Generic;
using System.Linq;
using AethersenseReduxReborn.Buttplug;
using AethersenseReduxReborn.Configurations;
using AethersenseReduxReborn.Signals.SignalGroup;
using Dalamud.Plugin.Services;

namespace AethersenseReduxReborn;

public sealed class SignalService: IDisposable
{
    private readonly ButtplugWrapper           _buttplugWrapper;
    private readonly SignalPluginConfiguration _signalPluginConfiguration;
    public           List<SignalGroup>         SignalGroups { get; } = new();

    public SignalService(ButtplugWrapper buttplugWrapper, SignalPluginConfiguration signalPluginConfiguration)
    {
        _buttplugWrapper                      =  buttplugWrapper;
        _signalPluginConfiguration            =  signalPluginConfiguration;
        Service.Framework.Update              += FrameworkUpdate;
        _buttplugWrapper.ActuatorAddedEvent   += ActuatorAdded;
        _buttplugWrapper.ActuatorRemovedEvent += ActuatorRemoved;
        _buttplugWrapper.ServerConnectedEvent += ServerConnected;

        ApplyConfiguration();
    }

    public void ApplyConfiguration()
    {
        foreach (var signalGroup in SignalGroups){
            signalGroup.Dispose();
        }
        SignalGroups.Clear();
        foreach (var groupConfig in _signalPluginConfiguration.SignalConfigurations){
            Service.PluginLog.Verbose("Applying new Signal Group. Name: {0}, CombineType: {1}, HashOfLastAssignedActuator: ", groupConfig.Name, groupConfig.CombineType, groupConfig.HashOfLastAssignedActuator?.ToString() ?? "");
            var signalGroup = new SignalGroup(groupConfig);
            SignalGroups.Add(signalGroup);
            if (signalGroup.HashOfLastAssignedActuator is null || !_buttplugWrapper.Actuators.ContainsKey(signalGroup.HashOfLastAssignedActuator))
                continue;
            signalGroup.Enable();
        }
        SignalGroups.TrimExcess();
    }

    public void SaveConfiguration()
    {
        Service.ConfigurationService.SaveSignalConfiguration(_signalPluginConfiguration);
        ApplyConfiguration();
    }

    public void Dispose()
    {
        Service.Framework.Update              -= FrameworkUpdate;
        _buttplugWrapper.ActuatorAddedEvent   -= ActuatorAdded;
        _buttplugWrapper.ActuatorRemovedEvent -= ActuatorRemoved;
        _buttplugWrapper.ServerConnectedEvent -= ServerConnected;
    }

    private void FrameworkUpdate(IFramework framework)
    {
        if (_buttplugWrapper.Connected == false)
            return;
        foreach (var signalGroup in SignalGroups.Where(signalGroup => signalGroup.Enabled)){
            signalGroup.UpdateSources(framework.UpdateDelta.TotalMilliseconds);
            try{
                if (signalGroup is {
                        Enabled: true, HashOfAssignedActuator: not null,
                    })
                    _buttplugWrapper.SendCommandToActuator(signalGroup.HashOfAssignedActuator, signalGroup.Signal);
            } catch (InvalidOperationException){
                signalGroup.Enabled = false;
            }
        }
    }

    private void ActuatorAdded(object? sender, ActuatorAddedEventArgs args)
    {
        foreach (var signalGroup in SignalGroups){
            if (signalGroup.HashOfLastAssignedActuator != args.HashOfActuator)
                continue;
            signalGroup.Enable(args.HashOfActuator);
        }
    }

    private void ActuatorRemoved(object? sender, ActuatorRemovedEventArgs args)
    {
        foreach (var signalGroup in SignalGroups){
            if (signalGroup.HashOfAssignedActuator != args.HashOfActuator)
                continue;
            signalGroup.Disable();
        }
    }

    private void ServerConnected(object? sender, EventArgs args) { ApplyConfiguration(); }
}
