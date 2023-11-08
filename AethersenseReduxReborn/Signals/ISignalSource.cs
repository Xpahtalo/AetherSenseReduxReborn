using System;
using AethersenseReduxReborn.Signals.Configs;

namespace AethersenseReduxReborn.Signals;

public interface ISignalSource: IDisposable
{
    public string       Name   { get; set; }
    public SignalOutput Output { get; }

    public void Update(double elapsedMilliseconds);

    public SignalSourceConfig CreateConfig();
}
