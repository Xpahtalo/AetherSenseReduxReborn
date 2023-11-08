using System;
using System.Numerics;
using AethersenseReduxReborn.Signals.Configs;

namespace AethersenseReduxReborn.Signals;

[Serializable]
public class SimplePattern
{
    private double _elapsedTime;

    private readonly SimplePatternType _patternType;

    private readonly long   _totalDuration;
    private readonly long   _duration1;
    private readonly long   _duration2;
    private readonly double _intensity1;
    private readonly double _intensity2;

    public bool IsCompleted { get; set; }

    public SimplePattern(SimplePatternType patternType, long totalDuration, long duration1, long duration2, double intensity1, double intensity2)
    {
        _patternType   = patternType;
        _totalDuration = totalDuration;
        _duration1     = duration1;
        _duration2     = duration2;
        _intensity1    = intensity1;
        _intensity2    = intensity2;
    }

    public SignalOutput Update(double elapsedMilliseconds)
    {
        _elapsedTime += elapsedMilliseconds;
        var weight = double.Clamp(_elapsedTime / _totalDuration, 0, 1);

        if (_elapsedTime >= _totalDuration)
            IsCompleted = true;

        var value = _patternType switch {
            SimplePatternType.Constant => _intensity1,
            SimplePatternType.Ramp     => Lerp(_intensity1, _intensity2, weight),
            SimplePatternType.Saw      => Lerp(_intensity1, _intensity2, weight % 1),
            SimplePatternType.Square   => _elapsedTime                          % _duration1 + _duration2 < _duration1 ? _intensity1 : _intensity2,
            SimplePatternType.Random   => Lerp(_intensity1, _intensity2, Random.Shared.NextDouble()),
            _                          => 0,
        };
        var output = new SignalOutput(value);
        Service.PluginLog.Verbose("time:{0}, weight:{1}, value:{2}", _elapsedTime, weight, output);
        return output;
    }

    private static T Lerp<T>(T start, T end, T weight)
        where T: INumber<T>, INumberBase<T> =>
        start * (T.One - weight) + end * weight;

    public static SimplePattern CreatePatternFromConfig(SimplePatternConfig patternConfig) =>
        new(patternConfig.PatternType,
            patternConfig.TotalDuration,
            patternConfig.Duration1,
            patternConfig.Duration2,
            patternConfig.Intensity1,
            patternConfig.Intensity2);
}

public enum SimplePatternType
{
    Constant,
    Ramp,
    Saw,
    Square,
    Random,
}
