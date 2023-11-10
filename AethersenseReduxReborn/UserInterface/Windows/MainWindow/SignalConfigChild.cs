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
    private TextInput                              NameInput                 { get; }
    private SingleSelectionCombo<ActuatorHash>     ActuatorCombo             { get; }
    private SingleSelectionCombo<CombineType>      CombineTypeCombo          { get; }
    private SingleSelectionCombo<SignalSourceType> SignalSourceTypeCombo     { get; }
    private Button                                 AddSignalSourceButton     { get; }
    private List<SignalSourceConfigEntry>          SignalSourceConfigEntries { get; } = new();

    private SignalGroupConfiguration GroupConfiguration    { get; }
    private SignalService            SignalService         { get; }
    private ButtplugWrapper          ButtplugWrapper       { get; }
    public  SignalSourceType         SignalSourceTypeToAdd { get; private set; }

    public SignalConfigChild(SignalGroupConfiguration signalGroupConfiguration, SignalService signalService, ButtplugWrapper buttplugButtplugWrapper)
    {
        GroupConfiguration = signalGroupConfiguration;
        SignalService      = signalService;
        ButtplugWrapper    = buttplugButtplugWrapper;

        NameInput = new TextInput("Name",
                                  100,
                                  s => GroupConfiguration.Name = s);
        ActuatorCombo = new SingleSelectionCombo<ActuatorHash>("Assigned Actuator",
                                                               hash => ButtplugWrapper.GetActuatorDisplayName(hash),
                                                               (hash1, hash2) => hash1 == hash2,
                                                               selection =>
                                                               {
                                                                   if (selection is null){
                                                                       return;
                                                                   }
                                                                   if (selection == ActuatorHash.Zeroed){
                                                                       return;
                                                                   }
                                                                   Service.PluginLog.Debug("Signal Group {0} selected new actuator {1}", GroupConfiguration.Name, selection);
                                                                   GroupConfiguration.HashOfLastAssignedActuator = selection;
                                                               });
        CombineTypeCombo = new SingleSelectionCombo<CombineType>("CombineType",
                                                                 combineType => combineType.ToString(),
                                                                 (combineType1, combineType2) => combineType1 == combineType2,
                                                                 selection => GroupConfiguration.CombineType = selection);


        SignalSourceTypeCombo = new SingleSelectionCombo<SignalSourceType>("Type to add",
                                                                           type => type.DisplayString(),
                                                                           (type1, type2) => type1 == type2,
                                                                           selection => SignalSourceTypeToAdd = selection);
        ImGui.SameLine();
        AddSignalSourceButton = new Button("Add",
                                           () =>
                                           {
                                               SignalSourceConfig config = SignalSourceTypeToAdd switch {
                                                   SignalSourceType.ChatTrigger     => ChatTriggerSignalConfig.EmptyConfig(),
                                                   SignalSourceType.PlayerAttribute => CharacterAttributeSignalConfig.EmptyConfig(),
                                                   _ => throw new ArgumentOutOfRangeException(nameof(SignalSourceTypeToAdd), SignalSourceTypeToAdd, "Unknown SignalSourceType while trying to add new SignalSourceConfig.") {
                                                       Source = "SignalConfigChild._addSignalSourceButton, ButtonPressed Lambda",
                                                       Data = {
                                                           { "SignalConfigChildId", Id },
                                                           { "SignalGroupConfigurationName", GroupConfiguration.Name },
                                                           { "SignalSourceTypeToAdd", SignalSourceTypeToAdd },
                                                       },
                                                   },
                                               };
                                               Service.PluginLog.Debug("SignalConfigChild: {0}: Add new SignalSourceConfig of type {1}", GroupConfiguration.Name, SignalSourceTypeToAdd);
                                               GroupConfiguration.SignalSources.Add(config);
                                               AddNewConfigEntry(config);
                                           });

        foreach (var signalSourceConfig in GroupConfiguration.SignalSources){
            AddNewConfigEntry(signalSourceConfig);
        }
    }

    public void Draw()
    {
        using var id    = ImRaii.PushId(Id.ToString());
        using var child = ImRaii.Child("SignalConfigChild");
        NameInput.Draw(GroupConfiguration.Name);
        ActuatorCombo.Draw(GroupConfiguration.HashOfLastAssignedActuator, ButtplugWrapper.Actuators.Select(actuator => actuator.Hash));
        CombineTypeCombo.Draw(GroupConfiguration.CombineType, Enum.GetValues<CombineType>());

        ImGui.Separator();
        ImGui.Text("Signal Sources");
        SignalSourceTypeCombo.Draw(SignalSourceTypeToAdd, Enum.GetValues<SignalSourceType>());
        ImGui.SameLine();
        AddSignalSourceButton.Draw();
        ImGui.Separator();
        ImGui.Separator();

        foreach (var entry in SignalSourceConfigEntries){
            entry.Draw();
            if (ImGui.Button($"Remove###{entry.Id}")){
                GroupConfiguration.SignalSources.Remove(entry.SignalSourceConfig);
                SignalSourceConfigEntries.Remove(entry);
            }
            ImGui.Separator();
        }
    }

    private void AddNewConfigEntry(SignalSourceConfig config)
    {
        SignalSourceConfigEntry entry = config switch {
            ChatTriggerSignalConfig chatTriggerSignalConfig            => new ChatTriggerConfigEntry(chatTriggerSignalConfig, SignalService, GroupConfiguration),
            CharacterAttributeSignalConfig playerAttributeSignalConfig => new PlayerAttributeConfigEntry(playerAttributeSignalConfig, SignalService),
            _ => throw new ArgumentOutOfRangeException(nameof(config), config, "Given SignalSourceConfig is of an unknown type.") {
                Source = "SignalConfigChild.AddNewConfigEntry",
                Data = {
                    { "SignalConfigChildId", Id },
                    { "SignalGroupConfigurationName", GroupConfiguration.Name },
                    { "SignalSourceConfigType", config.GetType() },
                },
            },
        };
        SignalSourceConfigEntries.Add(entry);
        Service.PluginLog.Debug("SignalConfigChild: {0}: Added source config to entry list: {1}", GroupConfiguration.Name, entry.SignalSourceConfig.Name);
    }
}
