using System;
using AethersenseReduxReborn.Signals.Configs;

namespace AethersenseReduxReborn.Signals;

public abstract class SignalBase: ISignalSource
{
    private double _value;

    public string Name { get; set; }

    public double Value {
        get => _value;
        protected set => _value = double.Clamp(value, 0, 1);
    }

    protected SignalBase(SignalSourceConfig config) { Name = config.Name; }

    public abstract void Update(double elapsedMilliseconds);

    public abstract SignalSourceConfig CreateConfig();

    protected virtual void Dispose(bool disposing)
    {
        if (disposing){ }
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
