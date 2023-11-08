using AethersenseReduxReborn.Signals;

namespace AethersenseReduxReborn.Buttplug;

public readonly record struct ActuatorCommand
{
    private readonly double _value;
    public double Value {
        get => _value;
        private init => _value = double.Clamp(value, 0, 1);
    }

    private ActuatorCommand(double value) { Value = value; }

    public ActuatorCommand(SignalOutput signalOutput) { Value = signalOutput.Value; }

    public ActuatorCommand Quantized(uint steps) => new(Quantize(Value, steps));

    public override string ToString() => Value.ToString("F2");

    private static double Quantize(double value, uint steps) => double.Round(steps * double.Clamp(value, 0, 1)) / steps;
}
