using System;
using System.Collections.Generic;
using System.Numerics;
using AethersenseReduxReborn.Buttplug;
using AethersenseReduxReborn.Configurations;
using AethersenseReduxReborn.Signals;
using AethersenseReduxReborn.Signals.SignalGroup;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using XIVChatTools;
using XpahtaLib.UserInterface;
using XpahtaLib.UserInterface.Input;
using XpahtaLib.UserInterface.Tabs;

namespace AethersenseReduxReborn.UserInterface.Windows.MainWindow;

public class SignalGroupTab: TabBase
{
    private          SignalGroupConfiguration? _selectedSignalGroupConfig;
    private readonly SignalPluginConfiguration _signalPluginConfiguration;

    private readonly SingleSelectionList<SignalGroupConfiguration> _signalGroupList;
    private readonly Button                                        _removeSignalGroupButton;
    private readonly Button                                        _addSignalGroupButton;
    private readonly Button                                        _saveConfigurationButton;
    private readonly Button                                        _applyConfigurationButton;
    private          SignalConfigChild?                            _signalConfigChild;

    private readonly ButtplugWrapper _buttplugWrapper;
    private readonly SignalService   _signalService;

    public override string Name => "Signal Groups";

    public SignalGroupTab(ButtplugWrapper buttplugWrapper, SignalService signalService)
    {
        _buttplugWrapper           = buttplugWrapper;
        _signalService             = signalService;
        _signalPluginConfiguration = Service.ConfigurationService.SignalPluginConfiguration;

        _signalGroupList = new SingleSelectionList<SignalGroupConfiguration>("Signal Groups",
                                                                             config => config.Name,
                                                                             (config1, config2) => config1 == config2,
                                                                             selectedConfig =>
                                                                             {
                                                                                 _selectedSignalGroupConfig = selectedConfig;
                                                                                 if (_selectedSignalGroupConfig is not null){
                                                                                     Service.PluginLog.Verbose("SignalGroupTab: Selected SignalGroupConfiguration: {0}", _selectedSignalGroupConfig.Name);
                                                                                     _signalConfigChild = new SignalConfigChild(_selectedSignalGroupConfig, _signalService, _buttplugWrapper);
                                                                                 }
                                                                             });
        _removeSignalGroupButton = new Button("Remove",
                                              () =>
                                              {
                                                  _signalConfigChild = null;
                                                  if (_selectedSignalGroupConfig is not null)
                                                      _signalPluginConfiguration.SignalConfigurations.Remove(_selectedSignalGroupConfig);
                                              });
        _addSignalGroupButton = new Button("Add",
                                           () =>
                                           {
                                               _signalPluginConfiguration.SignalConfigurations.Add(new SignalGroupConfiguration {
                                                   CombineType   = CombineType.Max,
                                                   Name          = "New Signal Group",
                                                   SignalSources = new List<SignalSourceConfig>(),
                                               });
                                               Service.PluginLog.Verbose("SignalGroupTab: Add new SignalGroupConfiguration");
                                           });
        _saveConfigurationButton  = new Button("Save",  () => _signalService.SaveConfiguration());
        _applyConfigurationButton = new Button("Apply", () => _signalService.ApplyConfiguration());
    }

    protected override void DrawTab()
    {
        var availableRegion = ImGui.GetContentRegionAvail();
        DrawSignalGroupList();
        ImGui.SameLine();
        DrawSelectedGroup();
        return;

        void DrawSignalGroupList()
        {
            using var listChild = ImRaii.Child("###GroupListChild", new Vector2(availableRegion.X * 0.25f, 0), true);
            if (!listChild)
                return;

            var listRegion = new Vector2 {
                X = ImGui.GetContentRegionAvail().X,
                Y = (ImGui.GetContentRegionAvail().Y / ImGui.GetTextLineHeightWithSpacing() - 1) * ImGui.GetTextLineHeightWithSpacing(),
            };
            _signalGroupList.Draw(_selectedSignalGroupConfig, _signalPluginConfiguration.SignalConfigurations, listRegion);


            _removeSignalGroupButton.Draw();
            ImGui.SameLine();
            _addSignalGroupButton.Draw();
            ImGui.SameLine();
            _applyConfigurationButton.Draw();
            ImGui.SameLine();
            _saveConfigurationButton.Draw();
        }

        void DrawSelectedGroup() { _signalConfigChild?.Draw(); }
    }
}

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
                                                                hash =>
                                                                {
                                                                    if (hash == ActuatorHash.Unassigned)
                                                                        return "Has not been assigned";
                                                                    return _buttplugWrapper.Actuators.TryGetValue(hash, out var actuator) ? actuator.DisplayName : "Actuator not connected.";
                                                                },
                                                                (hash1, hash2) => hash1 == hash2,
                                                                selection =>
                                                                {
                                                                    if (selection == ActuatorHash.Unassigned)
                                                                        return;
                                                                    Service.PluginLog.Verbose("Signal Group {0} selected new actuator {1}", _signalGroupConfiguration.Name, selection);
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
                                                Service.PluginLog.Verbose("SignalConfigChild: {0}: Add new SignalSourceConfig of type {1}", _signalGroupConfiguration.Name, _signalSourceTypeToAdd);
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
        _actuatorCombo.Draw(_signalGroupConfiguration.HashOfLastAssignedActuator, _buttplugWrapper.Actuators.Keys);
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
            ChatTriggerSignalConfig chatTriggerSignalConfig            => new ChatTriggerConfigEntry(chatTriggerSignalConfig),
            CharacterAttributeSignalConfig playerAttributeSignalConfig => new PlayerAttributeConfigEntry(playerAttributeSignalConfig),
            _                                                          => throw new ArgumentOutOfRangeException(nameof(config), config, null),
        };
        _signalSourceConfigEntries.Add(entry);
        Service.PluginLog.Verbose("SignalConfigChild: {0}: Added source config to entry list: {1}", _signalGroupConfiguration.Name, entry.SignalSourceConfig.Name);
    }
}

internal abstract class SignalSourceConfigEntry: ImGuiWidget
{
    private readonly TextInput          _nameInput;
    public readonly  SignalSourceConfig SignalSourceConfig;

    protected SignalSourceConfigEntry(SignalSourceConfig signalSourceConfig)
    {
        SignalSourceConfig = signalSourceConfig;
        _nameInput = new TextInput("Name",
                                   100,
                                   s => signalSourceConfig.Name = s);
    }

    public virtual void Draw()
    {
        using var id = ImRaii.PushId(Id.ToString());
        _nameInput.Draw(SignalSourceConfig.Name);
    }
}

internal class ChatTriggerConfigEntry: SignalSourceConfigEntry
{
    private readonly SingleSelectionCombo<Channel>           _chatChannelCombo;
    private readonly TextInput                               _regexInput;
    private readonly SingleSelectionCombo<SimplePatternType> _patternTypeCombo;
    private readonly IntInput                                _totalDurationInput;
    private readonly FloatSlider                             _intensity1Slider;
    private readonly FloatSlider                             _intensity2Slider;
    private readonly IntInput                                _duration1Input;
    private readonly IntInput                                _duration2Input;


    public ChatTriggerConfigEntry(ChatTriggerSignalConfig signalSourceConfig)
        : base(signalSourceConfig)
    {
        _chatChannelCombo = new SingleSelectionCombo<Channel>("Chat Channel",
                                                              channel => XivChatTypeEx.ChannelFriendlyName[channel],
                                                              (channel1, channel2) => channel1 == channel2,
                                                              selection => signalSourceConfig.ChatType = selection);
        _regexInput = new TextInput("RegexPattern",
                                    2048,
                                    pattern => signalSourceConfig.RegexPattern = pattern);
        _patternTypeCombo = new SingleSelectionCombo<SimplePatternType>("Pattern Type",
                                                                        type => type.ToString(),
                                                                        (type1, type2) => type1 == type2,
                                                                        selection => signalSourceConfig.PatternConfig.PatternType = selection);

        _totalDurationInput = new IntInput("Total Duration (ms)",
                                           i => signalSourceConfig.PatternConfig.TotalDuration = i,
                                           50);
        _intensity1Slider = new FloatSlider("Intensity 1",
                                            0.0f,
                                            1.0f,
                                            "%1.2f",
                                            f => signalSourceConfig.PatternConfig.Intensity1 = f);
        _intensity2Slider = new FloatSlider("Intensity 2",
                                            0.0f,
                                            1.0f,
                                            "%1.2f",
                                            f => signalSourceConfig.PatternConfig.Intensity2 = f);
        _duration1Input = new IntInput("Duration 1 (ms)",
                                       i => signalSourceConfig.PatternConfig.Duration1 = i,
                                       50);
        _duration2Input = new IntInput("Duration 2 (ms)",
                                       i => signalSourceConfig.PatternConfig.Duration2 = i,
                                       50);
    }

    public override void Draw()
    {
        base.Draw();
        using var id = ImRaii.PushId(Id.ToString());

        // Trigger config
        var chatTriggerSignalConfig = (SignalSourceConfig as ChatTriggerSignalConfig)!;
        _chatChannelCombo.Draw(chatTriggerSignalConfig.ChatType, Enum.GetValues<Channel>());
        _regexInput.Draw(chatTriggerSignalConfig.RegexPattern);

        // Pattern config
        var patternConfig = chatTriggerSignalConfig.PatternConfig;
        _patternTypeCombo.Draw(patternConfig.PatternType, Enum.GetValues<SimplePatternType>());
        _totalDurationInput.Draw(patternConfig.TotalDuration);
        switch (patternConfig.PatternType){
            case SimplePatternType.Constant:
                _intensity1Slider.Draw((float)patternConfig.Intensity1, "Intensity");
                break;
            case SimplePatternType.Ramp:
                _intensity1Slider.Draw((float)patternConfig.Intensity1, "Start Intensity");
                _intensity2Slider.Draw((float)patternConfig.Intensity2, "End Intensity");
                break;
            case SimplePatternType.Saw:
                _intensity1Slider.Draw((float)patternConfig.Intensity1, "Low Intensity");
                _intensity2Slider.Draw((float)patternConfig.Intensity2, "High Intensity");
                break;
            case SimplePatternType.Square:
                _intensity1Slider.Draw((float)patternConfig.Intensity1, "Low Intensity");
                _duration1Input.Draw(patternConfig.Duration1, "Low Duration (ms)");
                _intensity2Slider.Draw((float)patternConfig.Intensity2, "High Intensity");
                _duration2Input.Draw(patternConfig.Duration2, "High Duration (ms)");
                break;
            case SimplePatternType.Random:
                _intensity1Slider.Draw((float)patternConfig.Intensity1, "Low Intensity");
                _intensity2Slider.Draw((float)patternConfig.Intensity2, "High Intensity");
                break;
            default:
                ImGui.Text("Unknown Pattern Type");
                break;
        }
    }
}

internal class PlayerAttributeConfigEntry: SignalSourceConfigEntry
{
    private readonly TextInput                              _playerNameInput;
    private readonly SingleSelectionCombo<AttributeToTrack> _attributeToTrackCombo;
    private readonly SingleSelectionCombo<Correlation>      _correlationCombo;

    public PlayerAttributeConfigEntry(CharacterAttributeSignalConfig signalSourceConfig)
        : base(signalSourceConfig)
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
        _playerNameInput.Draw(attributeSignalConfig.Name);
        _attributeToTrackCombo.Draw(attributeSignalConfig.AttributeToTrack, Enum.GetValues<AttributeToTrack>());
        _correlationCombo.Draw(attributeSignalConfig.Correlation, Enum.GetValues<Correlation>());
    }
}
