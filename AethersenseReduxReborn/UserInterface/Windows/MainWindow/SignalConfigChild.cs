using System;
using System.Collections.Generic;
using System.Linq;
using AethersenseReduxReborn.Buttplug;
using AethersenseReduxReborn.Signals;
using AethersenseReduxReborn.Signals.Configs;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using XpahtaLib.UserInterface;
using XpahtaLib.UserInterface.Input;

namespace AethersenseReduxReborn.UserInterface.Windows.MainWindow;

internal class SignalConfigChild: ImGuiWidget
{
    private readonly TextInput                              _nameInput;
    private readonly SingleSelectionCombo<ActuatorHash>     _actuatorCombo;
    private readonly SingleSelectionCombo<CombineType>      _combineTypeCombo;
    private readonly SingleSelectionCombo<SignalSourceType> _signalSourceTypeCombo;
    private          SignalSourceType                       _signalSourceTypeToAdd;
    private readonly Button                                 _addSignalSourceButton;
    private readonly List<SignalSourceConfigEntry>          _signalSourceConfigEntries = new();

    private readonly SignalGroupConfiguration _signalGroupConfiguration;
    private readonly SignalService            _signalService;
    private readonly ButtplugWrapper          _buttplugWrapper;

    public SignalConfigChild(SignalGroupConfiguration signalGroupConfiguration, SignalService signalService, ButtplugWrapper buttplugWrapper)
    {
        _signalGroupConfiguration = signalGroupConfiguration;
        _signalService            = signalService;
        _buttplugWrapper          = buttplugWrapper;

        _nameInput = new TextInput("Name",
                                   100,
                                   s => _signalGroupConfiguration.Name = s);
        _actuatorCombo = new SingleSelectionCombo<ActuatorHash>("Assigned Actuator",
                                                                hash => _buttplugWrapper.GetActuatorDisplayName(hash),
                                                                (hash1, hash2) => hash1 == hash2,
                                                                selection =>
                                                                {
                                                                    if (selection is null)
                                                                        return;
                                                                    if (selection == ActuatorHash.Zeroed)
                                                                        return;
                                                                    Service.PluginLog.Debug("Signal Group {0} selected new actuator {1}", _signalGroupConfiguration.Name, selection);
                                                                    _signalGroupConfiguration.HashOfLastAssignedActuator = selection;
                                                                });
        _combineTypeCombo = new SingleSelectionCombo<CombineType>("CombineType",
                                                                  combineType => combineType.ToString(),
                                                                  (combineType1, combineType2) => combineType1 == combineType2,
                                                                  selection => _signalGroupConfiguration.CombineType = selection);


        _signalSourceTypeCombo = new SingleSelectionCombo<SignalSourceType>("Type to add",
                                                                            type => type.DisplayString(),
                                                                            (type1, type2) => type1 == type2,
                                                                            selection => _signalSourceTypeToAdd = selection);
        ImGui.SameLine();
        _addSignalSourceButton = new Button("Add",
                                            () =>
                                            {
                                                SignalSourceConfig config = _signalSourceTypeToAdd switch {
                                                    SignalSourceType.ChatTrigger     => ChatTriggerSignalConfig.DefaultConfig(),
                                                    SignalSourceType.PlayerAttribute => CharacterAttributeSignalConfig.DefaultConfig(),
                                                    _                                => throw new ArgumentOutOfRangeException(),
                                                };
                                                Service.PluginLog.Debug("SignalConfigChild: {0}: Add new SignalSourceConfig of type {1}", _signalGroupConfiguration.Name, _signalSourceTypeToAdd);
                                                _signalGroupConfiguration.SignalSources.Add(config);
                                                AddNewConfigEntry(config);
                                            });

        foreach (var signalSourceConfig in _signalGroupConfiguration.SignalSources){
            AddNewConfigEntry(signalSourceConfig);
        }
    }

    public void Draw()
    {
        using var id    = ImRaii.PushId(Id.ToString());
        using var child = ImRaii.Child("SignalConfigChild");
        _nameInput.Draw(_signalGroupConfiguration.Name);
        _actuatorCombo.Draw(_signalGroupConfiguration.HashOfLastAssignedActuator, _buttplugWrapper.Actuators.Select(actuator => actuator.Hash));
        _combineTypeCombo.Draw(_signalGroupConfiguration.CombineType, Enum.GetValues<CombineType>());

        ImGui.Separator();
        ImGui.Text("Signal Sources");
        _signalSourceTypeCombo.Draw(_signalSourceTypeToAdd, Enum.GetValues<SignalSourceType>());
        ImGui.SameLine();
        _addSignalSourceButton.Draw();
        ImGui.Separator();
        ImGui.Separator();

        foreach (var entry in _signalSourceConfigEntries){
            entry.Draw();
            if (ImGui.Button($"Remove###{entry.Id}")){
                _signalGroupConfiguration.SignalSources.Remove(entry.SignalSourceConfig);
                _signalSourceConfigEntries.Remove(entry);
            }
            ImGui.Separator();
        }
    }

    private void AddNewConfigEntry(SignalSourceConfig config)
    {
        SignalSourceConfigEntry entry = config switch {
            ChatTriggerSignalConfig chatTriggerSignalConfig            => new ChatTriggerConfigEntry(chatTriggerSignalConfig, _signalService, _signalGroupConfiguration),
            CharacterAttributeSignalConfig playerAttributeSignalConfig => new PlayerAttributeConfigEntry(playerAttributeSignalConfig, _signalService),
            _                                                          => throw new ArgumentOutOfRangeException(nameof(config), config, null),
        };
        _signalSourceConfigEntries.Add(entry);
        Service.PluginLog.Debug("SignalConfigChild: {0}: Added source config to entry list: {1}", _signalGroupConfiguration.Name, entry.SignalSourceConfig.Name);
    }
}
