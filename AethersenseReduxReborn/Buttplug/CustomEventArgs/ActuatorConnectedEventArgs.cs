using System;

namespace AethersenseReduxReborn.Buttplug.CustomEventArgs;

public class ActuatorConnectedEventArgs: EventArgs
{
    public required ActuatorHash HashOfActuator { get; init; }
}

public delegate void ActuatorConnectedEventHandler(ActuatorConnectedEventArgs args);
