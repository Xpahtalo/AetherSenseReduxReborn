using System;

namespace AethersenseReduxReborn.Buttplug.CustomEventArgs;

public class ActuatorAddedEventArgs: EventArgs
{
    public required ActuatorHash HashOfActuator { get; init; }
}
