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
    private          SignalPluginConfiguration _signalPluginConfiguration;
    public           IEnumerable<SignalGroup>  SignalGroups { get; } = new List<SignalGroup>();

    public SignalService(ButtplugWrapper buttplugWrapper, SignalPluginConfiguration signalPluginConfiguration)
    {
        _buttplugWrapper                      =  buttplugWrapper;
        _signalPluginConfiguration            =  signalPluginConfiguration;
        Service.Framework.Update              += FrameworkUpdate;
        _buttplugWrapper.ActuatorAddedEvent   += ActuatorAdded;
        _buttplugWrapper.ActuatorRemovedEvent += ActuatorRemoved;
    }

    public void SaveConfiguration(SignalPluginConfiguration signalPluginConfiguration)
    {
        _signalPluginConfiguration = signalPluginConfiguration;
        Service.ConfigurationService.SaveSignalConfiguration(_signalPluginConfiguration);
    }

    public void Dispose() { Service.Framework.Update -= FrameworkUpdate; }

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
            signalGroup.HashOfAssignedActuator = args.HashOfActuator;
            signalGroup.Enabled                = true;
        }
    }

    private void ActuatorRemoved(object? sender, ActuatorRemovedEventArgs args)
    {
        foreach (var signalGroup in SignalGroups){
            if (signalGroup.HashOfAssignedActuator != args.HashOfActuator)
                continue;
            signalGroup.HashOfAssignedActuator = null;
            signalGroup.Enabled                = false;
        }
    }
}
