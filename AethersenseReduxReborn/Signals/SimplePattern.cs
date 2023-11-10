using System;
using System.Numerics;
using AethersenseReduxReborn.Signals.Configs;

namespace AethersenseReduxReborn.Signals;

[Serializable]
public class SimplePattern
{
    public double            ElapsedTime   { get; private set; }
    public SimplePatternType PatternType   { get; }
    public long              TotalDuration { get; }
    public long              Duration1     { get; }
    public long              Duration2     { get; }
    public double            Intensity1    { get; }
    public double            Intensity2    { get; }

    public bool IsCompleted { get; set; }

    public SimplePattern(SimplePatternConfig patternConfig)
    {
        PatternType   = patternConfig.PatternType;
        TotalDuration = patternConfig.TotalDuration;
        Duration1     = patternConfig.Duration1;
        Duration2     = patternConfig.Duration2;
        Intensity1    = patternConfig.Intensity1;
        Intensity2    = patternConfig.Intensity2;
    }

    public SignalOutput Update(double elapsedMilliseconds)
    {
        ElapsedTime += elapsedMilliseconds;
        var weight = double.Clamp(ElapsedTime / TotalDuration, 0, 1);

        if (ElapsedTime >= TotalDuration){
            IsCompleted = true;
        }

        var value = PatternType switch {
            SimplePatternType.Constant => Intensity1,
            SimplePatternType.Ramp     => Lerp(Intensity1, Intensity2, weight),
            SimplePatternType.Saw      => Lerp(Intensity1, Intensity2, weight % 1),
            SimplePatternType.Square   => ElapsedTime                         % Duration1 + Duration2 < Duration1 ? Intensity1 : Intensity2,
            SimplePatternType.Random   => Lerp(Intensity1, Intensity2, Random.Shared.NextDouble()),
            _                          => 0,
        };
        var output = new SignalOutput(value);
        Service.PluginLog.Verbose("time:{0}, weight:{1}, value:{2}", ElapsedTime, weight, output);
        return output;
    }

    private static T Lerp<T>(T start, T end, T weight)
        where T: INumber<T>, INumberBase<T> =>
        start * (T.One - weight) + end * weight;
}

public enum SimplePatternType
{
    Constant,
    Ramp,
    Saw,
    Square,
    Random,
}
