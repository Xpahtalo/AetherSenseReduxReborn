using System;

namespace AethersenseReduxReborn.Signals;

public interface ISignalSource: IDisposable
{
    public string Name  { get; set; }
    public double Value { get; }

    public void Update(double elapsedMilliseconds);
}
