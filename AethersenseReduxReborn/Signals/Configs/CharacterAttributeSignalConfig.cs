namespace AethersenseReduxReborn.Signals.Configs;

public class CharacterAttributeSignalConfig: SignalSourceConfig
{
    public required string           CharacterName    { get; set; }
    public required AttributeToTrack AttributeToTrack { get; set; }
    public required Correlation      Correlation      { get; set; }

    public static CharacterAttributeSignalConfig EmptyConfig() =>
        new() {
            Name             = "",
            CharacterName    = "",
            AttributeToTrack = AttributeToTrack.Hp,
            Correlation      = Correlation.Positive,
        };
}
