using System;

namespace AethersenseReduxReborn.Buttplug.CustomEventArgs;

public class ActuatorRemovedEventArgs: EventArgs
{
    public required ActuatorHash HashOfActuator { get; init; }
}
