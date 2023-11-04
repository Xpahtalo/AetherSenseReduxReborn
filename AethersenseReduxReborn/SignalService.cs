using System;
using System.Collections.Generic;
using System.Linq;
using AethersenseReduxReborn.Buttplug;
using AethersenseReduxReborn.Buttplug.CustomEventArgs;
using AethersenseReduxReborn.Configurations;
using AethersenseReduxReborn.Signals;
using AethersenseReduxReborn.Signals.SignalGroup;
using Dalamud.Plugin.Services;

namespace AethersenseReduxReborn;

public sealed class SignalService: IDisposable
{
    private readonly ButtplugWrapper           _buttplugWrapper;
    private readonly SignalPluginConfiguration _signalPluginConfiguration;
    private          SimplePattern?            _testPattern;
    private          ActuatorHash              _testHash = ActuatorHash.Zeroed;

    public List<SignalGroup> SignalGroups { get; } = new();


    public SignalService(ButtplugWrapper buttplugWrapper, SignalPluginConfiguration signalPluginConfiguration)
    {
        _buttplugWrapper                      =  buttplugWrapper;
        _signalPluginConfiguration            =  signalPluginConfiguration;
        Service.Framework.Update              += FrameworkUpdate;
        _buttplugWrapper.ActuatorConnected    += ActuatorAdded;
        _buttplugWrapper.ActuatorDisconnected += ActuatorRemoved;
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
            Service.PluginLog.Debug("Applying new Signal Group. Name: {0}, CombineType: {1}, HashOfLastAssignedActuator: {2}",
                                    groupConfig.Name,
                                    groupConfig.CombineType,
                                    groupConfig.HashOfLastAssignedActuator);
            var signalGroup = new SignalGroup(groupConfig);
            SignalGroups.Add(signalGroup);

            if (signalGroup.HashOfLastAssignedActuator != ActuatorHash.Zeroed && _buttplugWrapper.IsActuatorConnected(signalGroup.HashOfLastAssignedActuator))
                signalGroup.Enable();
        }
    }

    public void SaveConfiguration()
    {
        Service.ConfigurationService.SaveSignalConfiguration(_signalPluginConfiguration);
        ApplyConfiguration();
    }

    public void Dispose()
    {
        Service.Framework.Update              -= FrameworkUpdate;
        _buttplugWrapper.ActuatorConnected    -= ActuatorAdded;
        _buttplugWrapper.ActuatorDisconnected -= ActuatorRemoved;
        _buttplugWrapper.ServerConnectedEvent -= ServerConnected;
    }

    private void FrameworkUpdate(IFramework framework)
    {
        if (_testPattern != null && _testHash != ActuatorHash.Zeroed){
            var value = _testPattern.Update(framework.UpdateDelta.TotalMilliseconds);
            _buttplugWrapper.SendCommandToActuator(_testHash, value);
            if (!_testPattern.IsCompleted)
                return;
            _testPattern = null;
            _testHash    = ActuatorHash.Zeroed;
        } else{
            if (_buttplugWrapper.Connected == false)
                return;
            foreach (var signalGroup in SignalGroups.Where(signalGroup => signalGroup.Enabled)){
                signalGroup.UpdateSources(framework.UpdateDelta.TotalMilliseconds);
                try{
                    if (signalGroup.Enabled && signalGroup.HashOfAssignedActuator != ActuatorHash.Zeroed)
                        _buttplugWrapper.SendCommandToActuator(signalGroup.HashOfAssignedActuator, signalGroup.Signal);
                } catch (InvalidOperationException ex){
                    Service.PluginLog.Error(ex, "Error while sending command to actuator. ActuatorHash: {0}", signalGroup.HashOfAssignedActuator);
                    signalGroup.Enabled = false;
                }
            }
        }
    }

    public void SetTestPattern(SimplePatternConfig pattern, ActuatorHash hash)
    {
        _testPattern = SimplePattern.CreatePatternFromConfig(pattern);
        _testHash    = hash;
    }

    private void ActuatorAdded(ActuatorAddedEventArgs args)
    {
        foreach (var signalGroup in SignalGroups){
            if (signalGroup.HashOfLastAssignedActuator == args.HashOfActuator)
                signalGroup.Enable(args.HashOfActuator);
        }
    }

    private void ActuatorRemoved(ActuatorRemovedEventArgs args)
    {
        foreach (var signalGroup in SignalGroups){
            if (signalGroup.HashOfAssignedActuator == args.HashOfActuator)
                signalGroup.Disable();
        }
    }

    private void ServerConnected(object? sender, EventArgs args) { ApplyConfiguration(); }
}
