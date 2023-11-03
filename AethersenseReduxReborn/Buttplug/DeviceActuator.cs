using Buttplug.Core.Messages;

namespace AethersenseReduxReborn.Buttplug;

public class DeviceActuator
{
    private double _previousValue;

    public uint         Index        { get; }
    public ActuatorType ActuatorType { get; }
    public string       Description  { get; }
    public uint         Steps        { get; }
    public Device       OwnerDevice  { get; }
    public ActuatorHash Hash         { get; }
    public string       DisplayName  => $"{OwnerDevice.Name} - {Index} - {ActuatorType} - {Description}";

    public DeviceActuator(Device ownerDevice, GenericDeviceMessageAttributes attributes)
    {
        Index        = attributes.Index;
        ActuatorType = attributes.ActuatorType;
        Description  = attributes.FeatureDescriptor;
        Steps        = attributes.StepCount;
        OwnerDevice  = ownerDevice;
        Hash         = ActuatorHash.ComputeHash(this);
    }

    public void SendCommand(double value)
    {
        var (quantizedValue, shouldSend) = ProcessValue(value);
        if (shouldSend)
            OwnerDevice.SendCommandToActuator(Index, quantizedValue);
    }

    private (double, bool) ProcessValue(double value)
    {
        var quantized = double.Round(Steps * value) / Steps;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        // Exact comparison is intentional.
        if (quantized == _previousValue)
            return (quantized, false);
        Service.PluginLog.Debug("Quantized value {0} to {1}", value, quantized);
        _previousValue = quantized;
        return (quantized, true);
    }
}
