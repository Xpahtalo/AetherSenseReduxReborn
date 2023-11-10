using System;
using System.Linq;
using AethersenseReduxReborn.Signals.Configs;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;

namespace AethersenseReduxReborn.Signals;

public sealed class CharacterAttributeSignal: SignalBase
{
    private string             CharacterName    { get; }
    private AttributeToTrack   AttributeToTrack { get; }
    private Correlation        Correlation      { get; }
    private CharacterSelection TargetType       { get; }

    public CharacterAttributeSignal(CharacterAttributeSignalConfig config)
        : base(config)
    {
        CharacterName    = config.CharacterName;
        AttributeToTrack = config.AttributeToTrack;
        Correlation      = config.Correlation;
        TargetType = CharacterName switch {
            "{target}" => CharacterSelection.Target,
            "{self}"   => CharacterSelection.Self,
            _          => CharacterSelection.ByName,
        };
    }

    public override void Update(double elapsedMilliseconds)
    {
        var gameObject = TargetType switch {
            CharacterSelection.Target => Service.TargetManager.Target,
            CharacterSelection.Self   => Service.ClientState.LocalPlayer,
            CharacterSelection.ByName => Service.ObjectTable.Single(o => o.Name.TextValue == CharacterName),
            _ => throw new ArgumentOutOfRangeException(nameof(TargetType), TargetType, "TargetType was an invalid value") {
                Data = {
                    { "TriggerName", Name },
                },
            },
        };

        if (gameObject is null || !gameObject.IsValid()){
            Output = SignalOutput.Zero;
            return;
        }

        double value;
        // ReSharper disable once ConvertSwitchStatementToSwitchExpression
        switch (gameObject.ObjectKind){
            case ObjectKind.Player:
                value = AttributeToTrack switch {
                    AttributeToTrack.Hp => (double)((PlayerCharacter)gameObject).CurrentHp / ((PlayerCharacter)gameObject).MaxHp,
                    AttributeToTrack.Mp => (double)((PlayerCharacter)gameObject).CurrentMp / ((PlayerCharacter)gameObject).MaxMp,
                    _ => throw new ArgumentOutOfRangeException(nameof(AttributeToTrack), AttributeToTrack, "AttributeToTrack was an invalid value") {
                        Source = "CharacterAttributeSignal.Update",
                        Data = {
                            { "TriggerName", Name },
                        },
                    },
                };
                break;
            case ObjectKind.BattleNpc or ObjectKind.EventNpc or ObjectKind.Companion:
                value = AttributeToTrack switch {
                    AttributeToTrack.Hp => (double)((Character)gameObject).CurrentHp / ((Character)gameObject).MaxHp,
                    AttributeToTrack.Mp => 0d,
                    _ => throw new ArgumentOutOfRangeException(nameof(AttributeToTrack), AttributeToTrack, "AttributeToTrack was an invalid value") {
                        Source = "CharacterAttributeSignal.Update",
                        Data = {
                            { "TriggerName", Name },
                        },
                    },
                };
                break;
            default:
                value = 0d;
                break;
        }

        if (Correlation == Correlation.Inverse){
            value = 1 - value;
        }
        Output = new SignalOutput(value);
    }

    public override SignalSourceConfig CreateConfig() =>
        new CharacterAttributeSignalConfig {
            Name             = Name,
            CharacterName    = CharacterName,
            AttributeToTrack = AttributeToTrack,
            Correlation      = Correlation,
        };
}

internal enum CharacterSelection
{
    Target,
    Self,
    ByName,
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
