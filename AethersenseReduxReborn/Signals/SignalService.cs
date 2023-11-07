using System;
using System.Collections.Generic;
using System.Linq;
using AethersenseReduxReborn.Buttplug;
using AethersenseReduxReborn.Configurations;
using AethersenseReduxReborn.Signals.Configs;
using Dalamud.Plugin.Services;

namespace AethersenseReduxReborn.Signals;

public sealed class SignalService: IDisposable
{
    private readonly ButtplugWrapper           _buttplugWrapper;
    private readonly SignalPluginConfiguration _signalPluginConfiguration;
    private          SimplePattern?            _testPattern;
    private          ActuatorHash              _testHash = ActuatorHash.Zeroed;
    private          SignalGroupCollection     _signalGroupCollection;

    public SignalService(ButtplugWrapper buttplugWrapper, SignalPluginConfiguration signalPluginConfiguration)
    {
        _buttplugWrapper           =  buttplugWrapper;
        _signalPluginConfiguration =  signalPluginConfiguration;
        Service.Framework.Update   += FrameworkUpdate;

        _signalGroupCollection = new SignalGroupCollection(signalPluginConfiguration.SignalConfigurations, buttplugWrapper);
    }

    public void ApplyConfiguration(IEnumerable<SignalGroupConfiguration> groupConfigurations)
    {
        _signalGroupCollection.Dispose();
        _signalGroupCollection = new SignalGroupCollection(groupConfigurations, _buttplugWrapper);
    }

    public void SaveConfiguration(IEnumerable<SignalGroupConfiguration> groupConfigurations)
    {
        var signalGroupConfigurations = groupConfigurations.ToList();
        _signalPluginConfiguration.SignalConfigurations = signalGroupConfigurations;
        Service.ConfigurationService.SaveSignalConfiguration(_signalPluginConfiguration);
        ApplyConfiguration(signalGroupConfigurations);
    }

    public void Dispose()
    {
        _signalGroupCollection.Dispose();
        Service.Framework.Update -= FrameworkUpdate;
    }

    private void FrameworkUpdate(IFramework framework)
    {
        if (_testPattern != null && _testHash != ActuatorHash.Zeroed){
            var value = _testPattern.Update(framework.UpdateDelta.TotalMilliseconds);
            _buttplugWrapper.SendCommandToActuator(_testHash, new ActuatorCommand(value));
            if (!_testPattern.IsCompleted)
                return;
            _testPattern = null;
            _testHash    = ActuatorHash.Zeroed;
        } else{
            _signalGroupCollection.UpdateSignalGroups(framework.UpdateDelta.TotalMilliseconds);
        }
    }

    public void SetTestPattern(SimplePatternConfig pattern, ActuatorHash hash)
    {
        _testPattern = SimplePattern.CreatePatternFromConfig(pattern);
        _testHash    = hash;
    }

    public IEnumerable<SignalGroupConfiguration> GetSignalGroupConfigurations() => _signalGroupCollection.CreateConfig();
}
