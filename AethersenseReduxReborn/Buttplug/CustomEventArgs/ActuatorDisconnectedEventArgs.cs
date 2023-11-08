using System;

namespace AethersenseReduxReborn.Buttplug.CustomEventArgs;

public class ActuatorDisconnectedEventArgs: EventArgs
{
    public required ActuatorHash HashOfActuator { get; init; }
}

public delegate void ActuatorDisconnectedEventHandler(ActuatorDisconnectedEventArgs args);
