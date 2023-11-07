using System;
using AethersenseReduxReborn.Signals;
using AethersenseReduxReborn.Signals.Configs;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using XIVChatTools;
using XpahtaLib.UserInterface;
using XpahtaLib.UserInterface.Input;

namespace AethersenseReduxReborn.UserInterface.Windows.MainWindow;

internal class ChatTriggerConfigEntry: SignalSourceConfigEntry
{
    private readonly SingleSelectionCombo<Channel>           _chatChannelCombo;
    private readonly TextInput                               _regexInput;
    private readonly Button                                  _regexHelperButton;
    private readonly SingleSelectionCombo<SimplePatternType> _patternTypeCombo;
    private readonly IntInput                                _totalDurationInput;
    private readonly FloatSlider                             _intensity1Slider;
    private readonly FloatSlider                             _intensity2Slider;
    private readonly IntInput                                _duration1Input;
    private readonly IntInput                                _duration2Input;
    private readonly Button                                  _testPatternButton;


    public ChatTriggerConfigEntry(ChatTriggerSignalConfig signalSourceConfig, SignalService signalService, SignalGroupConfiguration signalGroupConfiguration)
        : base(signalSourceConfig, signalService)
    {
        _chatChannelCombo = new SingleSelectionCombo<Channel>("Chat Channel",
                                                              channel => XivChatTypeEx.ChannelFriendlyName[channel],
                                                              (channel1, channel2) => channel1 == channel2,
                                                              selection => signalSourceConfig.ChatType = selection);
        _regexInput = new TextInput("RegexPattern",
                                    2048,
                                    pattern => signalSourceConfig.RegexPattern = pattern);
        _regexHelperButton = new Button("Help",
                                        () =>
                                        {
                                            var regexHelper = new RegexHelper($"Regex Helper for {signalSourceConfig.Name}###{Guid.NewGuid()}",
                                                                              regexText => signalSourceConfig.RegexPattern = regexText);
                                            Service.WindowManager.AddWindow(regexHelper);
                                            Service.WindowManager.OpenWindow(regexHelper.WindowName);
                                        });
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
        _testPatternButton = new Button("Test",
                                        () => SignalService.SetTestPattern(signalSourceConfig.PatternConfig, signalGroupConfiguration.HashOfLastAssignedActuator));
    }

    public override void Draw()
    {
        base.Draw();
        using var id = ImRaii.PushId(Id.ToString());

        // Trigger config
        var chatTriggerSignalConfig = (SignalSourceConfig as ChatTriggerSignalConfig)!;
        _chatChannelCombo.Draw(chatTriggerSignalConfig.ChatType, Enum.GetValues<Channel>());
        _regexInput.Draw(chatTriggerSignalConfig.RegexPattern);
        _regexHelperButton.Draw();

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
        _testPatternButton.Draw();
        ImGui.SameLine();
    }
}
