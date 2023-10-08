using System.Collections.Generic;
using System.Linq;

namespace AethersenseReduxReborn.Signals.SignalGroup;

public class SignalGroup
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
            Service.PluginLog.Verbose(value ? "Enabling SignalGroup {0}" : "Disabling SignalGroup {0}", Name);
            _enabled = value;
        }
    }

    public SignalGroup(SignalGroupConfiguration groupConfiguration)
    {
        Name                       = groupConfiguration.Name;
        CombineType                = groupConfiguration.CombineType;
        HashOfLastAssignedActuator = groupConfiguration.HashOfLastAssignedActuator;
        Enabled                    = false;
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
        Service.PluginLog.Verbose("Adding new SignalSource to SignalGroup {0}", Name);
        SignalSources.Add(source);
    }

    public void RemoveSignalSource(ISignalSource signalSource)
    {
        Service.PluginLog.Verbose("Removing SignalSource from SignalGroup {0}", Name);
        SignalSources.Remove(signalSource);
    }
}
