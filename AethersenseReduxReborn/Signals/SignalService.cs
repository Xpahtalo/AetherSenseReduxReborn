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
    private SimplePattern?        TestPattern     { get; set; }
    private ActuatorHash          TestHash        { get; set; } = ActuatorHash.Zeroed;
    private SignalGroupCollection GroupCollection { get; set; }

    private readonly SignalPluginConfiguration _signalPluginConfiguration;

    public ButtplugWrapper ButtplugWrapper { get; }

    public SignalService()
    {
        _signalPluginConfiguration = Service.ConfigurationService.SignalPluginConfiguration;
        ButtplugWrapper            = new ButtplugWrapper(Plugin.Name, Service.ConfigurationService.PluginConfiguration);

        Service.Framework.Update += FrameworkUpdate;

        GroupCollection = new SignalGroupCollection(_signalPluginConfiguration.SignalConfigurations, ButtplugWrapper);
    }

    public void ApplyConfiguration(IEnumerable<SignalGroupConfiguration> groupConfigurations)
    {
        GroupCollection.Dispose();
        GroupCollection = new SignalGroupCollection(groupConfigurations, ButtplugWrapper);
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
        ButtplugWrapper.Dispose();
        GroupCollection.Dispose();
        Service.Framework.Update -= FrameworkUpdate;
    }

    private void FrameworkUpdate(IFramework framework)
    {
        if (TestPattern != null && TestHash != ActuatorHash.Zeroed){
            var value = TestPattern.Update(framework.UpdateDelta.TotalMilliseconds);
            ButtplugWrapper.SendCommandToActuator(TestHash, new ActuatorCommand(value));
            if (TestPattern.IsCompleted){
                TestPattern = null;
                TestHash    = ActuatorHash.Zeroed;
            }
        } else{
            GroupCollection.UpdateSignalGroups(framework.UpdateDelta.TotalMilliseconds);
        }
    }

    public void SetTestPattern(SimplePatternConfig pattern, ActuatorHash hash)
    {
        TestPattern = new SimplePattern(pattern);
//        _testPattern = SimplePattern.CreatePatternFromConfig(pattern);
        TestHash = hash;
    }

    public IEnumerable<SignalGroupConfiguration> GetSignalGroupConfigurations() => GroupCollection.CreateConfig();
}
