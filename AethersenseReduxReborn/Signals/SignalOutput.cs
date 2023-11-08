using System;

namespace AethersenseReduxReborn.Signals;

public readonly record struct SignalOutput:
    IComparable<double>,
    IComparable<SignalOutput>
{
    private readonly double _value;
    public double Value {
        get => _value;
        private init => _value = double.Clamp(value, 0, 1);
    }

    public static SignalOutput Zero => new(0d);

    public SignalOutput() { Value = 0d; }

    public SignalOutput(double value) { Value = value; }

    public int CompareTo(SignalOutput other) => _value.CompareTo(other._value);

    public int CompareTo(double other) => _value.CompareTo(other);

    public override string ToString() => Value.ToString("F2");

    public static implicit operator double(SignalOutput output) => output.Value;

    public static bool operator >(SignalOutput left, SignalOutput right) => left.CompareTo(right) > 0;

    public static bool operator <(SignalOutput left, SignalOutput right) => left.CompareTo(right) < 0;

    public static bool operator >=(SignalOutput left, SignalOutput right) => left.CompareTo(right) >= 0;

    public static bool operator <=(SignalOutput left, SignalOutput right) => left.CompareTo(right) <= 0;
}
