using System;
using AethersenseReduxReborn.Signals.Configs;

namespace AethersenseReduxReborn.Signals;

public abstract class SignalBase: ISignalSource
{
    public string Name { get; set; }

    public SignalOutput Output { get; protected set; }

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
