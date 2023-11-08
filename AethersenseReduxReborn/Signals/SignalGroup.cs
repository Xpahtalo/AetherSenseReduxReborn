using System;
using System.Collections.Generic;
using System.Linq;
using AethersenseReduxReborn.Buttplug;
using AethersenseReduxReborn.Signals.Configs;

namespace AethersenseReduxReborn.Signals;

public sealed class SignalGroup: IDisposable
{
    private bool         _enabled;
    private ActuatorHash _hashOfAssignedActuator = null!;

    public string              Name          { get; set; }
    public CombineType         CombineType   { get; set; }
    public SignalOutput        Signal        { get; set; }
    public List<ISignalSource> SignalSources { get; } = new();
    public ActuatorHash HashOfAssignedActuator {
        get => _hashOfAssignedActuator;
        set {
            _hashOfAssignedActuator = value;
            if (value != ActuatorHash.Zeroed)
                HashOfLastAssignedActuator = value;
        }
    }
    public ActuatorHash HashOfLastAssignedActuator { get; private set; }
    public bool Enabled {
        get => _enabled;
        private set {
            Service.PluginLog.Debug(value ? "Enabling SignalGroup {0}" : "Disabling SignalGroup {0}", Name);
            _enabled = value;
        }
    }

    public SignalGroup(SignalGroupConfiguration groupConfiguration)
    {
        Name                       = groupConfiguration.Name;
        CombineType                = groupConfiguration.CombineType;
        HashOfLastAssignedActuator = groupConfiguration.HashOfLastAssignedActuator;
        _enabled                   = false;

        foreach (var sourceConfig in groupConfiguration.SignalSources){
            ISignalSource source = sourceConfig switch {
                ChatTriggerSignalConfig chatTriggerSignalConfig            => new ChatTriggerSignal(chatTriggerSignalConfig),
                CharacterAttributeSignalConfig playerAttributeSignalConfig => new CharacterAttributeSignal(playerAttributeSignalConfig),
                _                                                          => throw new ArgumentOutOfRangeException(nameof(sourceConfig)),
            };
            Service.PluginLog.Debug("Adding new SignalSource {0} to SignalGroup {1}", sourceConfig.Name, Name);
            AddSignalSource(source);
        }
    }

    public void UpdateSources(double elapsedMilliseconds)
    {
        foreach (var signalSource in SignalSources){
            signalSource.Update(elapsedMilliseconds);
        }
        var activeSources = SignalSources.Where(source => source.Output > SignalOutput.Zero).ToList();
        if (activeSources.Count == 0){
            Signal = SignalOutput.Zero;
            return;
        }
        var intensity = CombineType switch {
            CombineType.Average => activeSources.Sum(source => source.Output) / activeSources.Count,
            CombineType.Max     => activeSources.Max(source => source.Output),
            CombineType.Minimum => activeSources.Min(source => source.Output),
            _                   => 0,
        };
        if (double.IsNaN(intensity))
            intensity = 0;
        Signal = new SignalOutput(intensity);
    }

    public void AddSignalSource(ISignalSource source)
    {
        Service.PluginLog.Debug("Adding new SignalSource to SignalGroup {0}", Name);
        SignalSources.Add(source);
    }

    public void RemoveSignalSource(ISignalSource signalSource)
    {
        Service.PluginLog.Debug("Removing SignalSource from SignalGroup {0}", Name);
        SignalSources.Remove(signalSource);
    }

    public SignalGroupConfiguration CreateConfiguration()
    {
        var signalSourceConfigs =
            from signalSource in SignalSources
            select signalSource.CreateConfig();

        return new SignalGroupConfiguration {
            Name                       = Name,
            CombineType                = CombineType,
            HashOfLastAssignedActuator = HashOfLastAssignedActuator,
            SignalSources              = signalSourceConfigs.ToList(),
        };
    }

    public void Enable() => Enable(HashOfLastAssignedActuator);

    public void Enable(ActuatorHash hash)
    {
        HashOfAssignedActuator = hash;
        Enabled                = true;
    }

    public void Disable()
    {
        HashOfAssignedActuator = ActuatorHash.Zeroed;
        Enabled                = false;
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
            foreach (var source in SignalSources){
                source.Dispose();
            }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
