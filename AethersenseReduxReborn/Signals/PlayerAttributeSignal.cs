using System;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Utility;

namespace AethersenseReduxReborn.Signals;

public sealed class PlayerAttributeSignal: SignalBase
{
    private readonly PlayerAttributeSignalConfig _config;

    public PlayerAttributeSignal(PlayerAttributeSignalConfig config) { _config = config; }

    public override void Update(double elapsedMilliseconds)
    {
        if (_config.PlayerName.IsNullOrWhitespace()){
            Value = 0;
            return;
        }

        var player = (PlayerCharacter)Service.ObjectTable.Single(o => o.Name.TextValue == _config.PlayerName);
        var val = _config.AttributeToTrack switch {
            AttributeToTrack.Hp => (double)player.CurrentHp / player.MaxHp,
            AttributeToTrack.Mp => (double)player.CurrentMp / player.MaxMp,
            _                   => throw new ArgumentOutOfRangeException(),
        };
        if (_config.Correlation == Correlation.Inverse)
            val = 1 - val;
        Value = val;
    }
}

public class PlayerAttributeSignalConfig: SignalSourceConfig
{
    public required string           PlayerName       { get; set; }
    public required AttributeToTrack AttributeToTrack { get; set; }
    public required Correlation      Correlation      { get; set; }

    public static PlayerAttributeSignalConfig DefaultConfig() =>
        new() {
            PlayerName       = "",
            AttributeToTrack = AttributeToTrack.Hp,
            Correlation      = Correlation.Positive,
        };
}

public enum AttributeToTrack
{
    Hp,
    Mp,
}

public enum Correlation
{
    Positive,
    Inverse,
}
