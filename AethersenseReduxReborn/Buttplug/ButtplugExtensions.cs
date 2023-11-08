using System;
using System.Collections.Generic;
using System.Linq;
using Buttplug.Client;
using Buttplug.Core.Messages;

namespace AethersenseReduxReborn.Buttplug;

public static class ButtplugClientDeviceExtensions
{
    public static IEnumerable<GenericDeviceMessageAttributes> GetGenericDeviceMessageAttributes(this ButtplugClientDevice device) => Enum.GetValues(typeof(ActuatorType)).Cast<ActuatorType>().SelectMany(device.GenericAcutatorAttributes);
}
