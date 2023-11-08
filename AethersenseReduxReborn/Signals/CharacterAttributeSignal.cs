using System;
using System.Linq;
using AethersenseReduxReborn.Signals.Configs;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;

namespace AethersenseReduxReborn.Signals;

public sealed class CharacterAttributeSignal: SignalBase
{
    private readonly string             _characterName;
    private readonly AttributeToTrack   _attributeToTrack;
    private readonly Correlation        _correlation;
    private readonly CharacterSelection _characterSelection;

    public CharacterAttributeSignal(CharacterAttributeSignalConfig config)
        : base(config)
    {
        _characterName    = config.CharacterName;
        _attributeToTrack = config.AttributeToTrack;
        _correlation      = config.Correlation;
        _characterSelection = _characterName switch {
            "{target}" => CharacterSelection.Target,
            "{self}"   => CharacterSelection.Self,
            _          => CharacterSelection.ByName,
        };
    }

    public override void Update(double elapsedMilliseconds)
    {
        var gameObject = _characterSelection switch {
            CharacterSelection.Target => Service.TargetManager.Target,
            CharacterSelection.Self   => Service.ClientState.LocalPlayer,
            CharacterSelection.ByName => Service.ObjectTable.Single(o => o.Name.TextValue == _characterName),
            _                         => throw new ArgumentOutOfRangeException(),
        };

        if (gameObject is null || !gameObject.IsValid()){
            Output = SignalOutput.Zero;
            return;
        }

        var value = gameObject.ObjectKind switch {
            ObjectKind.Player => _attributeToTrack switch {
                AttributeToTrack.Hp => (double)((PlayerCharacter)gameObject).CurrentHp / ((PlayerCharacter)gameObject).MaxHp,
                AttributeToTrack.Mp => (double)((PlayerCharacter)gameObject).CurrentMp / ((PlayerCharacter)gameObject).MaxMp,
                _                   => throw new ArgumentOutOfRangeException(),
            },
            ObjectKind.BattleNpc or ObjectKind.EventNpc or ObjectKind.Companion => _attributeToTrack switch {
                AttributeToTrack.Hp => (double)((Character)gameObject).CurrentHp / ((Character)gameObject).MaxHp,
                AttributeToTrack.Mp => 0d,
                _                   => throw new ArgumentOutOfRangeException(),
            },
            _ => 0d,
        };

        if (_correlation == Correlation.Inverse)
            value = 1 - value;
        Output = new SignalOutput(value);
    }

    public override SignalSourceConfig CreateConfig() =>
        new CharacterAttributeSignalConfig {
            Name             = Name,
            CharacterName    = _characterName,
            AttributeToTrack = _attributeToTrack,
            Correlation      = _correlation,
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
