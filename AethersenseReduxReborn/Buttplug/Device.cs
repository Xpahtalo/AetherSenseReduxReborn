using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buttplug.Client;
using Buttplug.Core.Messages;

namespace AethersenseReduxReborn.Buttplug;

public class Device
{
    private readonly ButtplugClientDevice _internalDevice;

    public string               Name      => _internalDevice.Name;
    public List<DeviceActuator> Actuators { get; }

    public Device(ButtplugClientDevice internalDevice)
    {
        _internalDevice = internalDevice;
        var internalList = new List<GenericDeviceMessageAttributes>();
        foreach (ActuatorType actuatorType in Enum.GetValues(typeof(ActuatorType))){
            internalList.AddRange(_internalDevice.GenericAcutatorAttributes(actuatorType));
        }


        Actuators = new List<DeviceActuator>();
        foreach (var actuator in internalList){
            Actuators.Add(new DeviceActuator(this, actuator));
        }
    }

    public void SendCommandToActuator(uint index, double value)
    {
        var actuator = Actuators.Single(actuator => actuator.Index == index);

        // Only send the new value if it has changed enough to result in a new response from the device. 
        Service.PluginLog.Debug("Sending value {0} to device {1} actuator {2}", value, Name, actuator.Description);
        Task.Run(async () => await _internalDevice.ScalarAsync(new ScalarCmd.ScalarSubcommand(actuator.Index, value, actuator.ActuatorType)));
    }
}
