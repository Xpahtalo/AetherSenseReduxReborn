using System;
using System.Collections.Generic;
using System.Linq;

namespace AethersenseReduxReborn.Signals.SignalGroup;

public class SignalGroup: IDisposable
{
    private bool    _enabled = true;
    private byte[]? _hashOfAssignedActuator;

    public string              Name          { get; set; }
    public CombineType         CombineType   { get; set; }
    public double              Signal        { get; set; }
    public List<ISignalSource> SignalSources { get; } = new();
    public byte[]? HashOfAssignedActuator {
        get => _hashOfAssignedActuator;
        set {
            _hashOfAssignedActuator = value;
            if (value is not null)
                HashOfLastAssignedActuator = value;
        }
    }
    public byte[]? HashOfLastAssignedActuator { get; set; }
    public bool Enabled {
        get => _enabled;
        set {
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
                ChatTriggerSignalConfig chatTriggerSignalConfig         => new ChatTriggerSignal(chatTriggerSignalConfig),
                CharacterAttributeSignalConfig playerAttributeSignalConfig => new CharacterAttributeSignal(playerAttributeSignalConfig),
                _                                                       => throw new ArgumentOutOfRangeException(nameof(sourceConfig)),
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
        var activeSources = SignalSources.Where(source => source.Value > 0).ToList();
        if (activeSources.Count == 0){
            Signal = 0;
            return;
        }
        var intensity = CombineType switch {
            CombineType.Average => activeSources.Sum(source => source.Value) / activeSources.Count,
            CombineType.Max     => activeSources.Max(source => source.Value),
            CombineType.Minimum => activeSources.Min(source => source.Value),
            _                   => 0,
        };
        if (double.IsNaN(intensity))
            intensity = 0;
        Signal = double.Clamp(intensity, 0, 1);
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

    public void Enable() => Enable(HashOfLastAssignedActuator ?? throw new InvalidOperationException());

    public void Enable(byte[] hash)
    {
        HashOfAssignedActuator = hash;
        Enabled                = true;
    }

    public void Disable()
    {
        HashOfAssignedActuator = null;
        Enabled                = false;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
            foreach (var source in SignalSources){
                source.Dispose();
            }
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
