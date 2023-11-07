using System;
using AethersenseReduxReborn.Signals;
using AethersenseReduxReborn.Signals.Configs;
using Dalamud.Interface.Utility.Raii;
using XpahtaLib.UserInterface;
using XpahtaLib.UserInterface.Input;

namespace AethersenseReduxReborn.UserInterface.Windows.MainWindow;

internal class PlayerAttributeConfigEntry: SignalSourceConfigEntry
{
    private readonly TextInput                              _playerNameInput;
    private readonly SingleSelectionCombo<AttributeToTrack> _attributeToTrackCombo;
    private readonly SingleSelectionCombo<Correlation>      _correlationCombo;

    public PlayerAttributeConfigEntry(CharacterAttributeSignalConfig signalSourceConfig, SignalService signalService)
        : base(signalSourceConfig, signalService)
    {
        _playerNameInput = new TextInput("Character Name",
                                         20,
                                         name => signalSourceConfig.CharacterName = name,
                                         "Use {target} if you want it to dynamically change to your current target.\nUse {self} for your own character.");
        _attributeToTrackCombo = new SingleSelectionCombo<AttributeToTrack>("Attribute to Track",
                                                                            attributeToTrack => attributeToTrack.ToString(),
                                                                            (attributeToTrack1, attributeToTrack2) => attributeToTrack1 == attributeToTrack2,
                                                                            selection => signalSourceConfig.AttributeToTrack = selection,
                                                                            "If using {target}, MP will do nothing when targeting enemies.");
        _correlationCombo = new SingleSelectionCombo<Correlation>("Correlation",
                                                                  correlation => correlation.ToString(),
                                                                  (correlation1, correlation2) => correlation1 == correlation2,
                                                                  selection => signalSourceConfig.Correlation = selection);
    }

    public override void Draw()
    {
        base.Draw();
        using var id = ImRaii.PushId(Id.ToString());

        var attributeSignalConfig = (SignalSourceConfig as CharacterAttributeSignalConfig)!;
        _playerNameInput.Draw(attributeSignalConfig.CharacterName);
        _attributeToTrackCombo.Draw(attributeSignalConfig.AttributeToTrack, Enum.GetValues<AttributeToTrack>());
        _correlationCombo.Draw(attributeSignalConfig.Correlation, Enum.GetValues<Correlation>());
    }
}
