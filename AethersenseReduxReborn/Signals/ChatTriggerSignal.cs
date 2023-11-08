using System;
using System.Text.RegularExpressions;
using AethersenseReduxReborn.Signals.Configs;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using XIVChatTools;

namespace AethersenseReduxReborn.Signals;

public class ChatTriggerSignal: SignalBase
{
    private          SimplePattern?      _currentPattern;
    private readonly Regex               _regex;
    private readonly Channel             _chatChannel;
    private readonly SimplePatternConfig _patternConfig;

    public ChatTriggerSignal(ChatTriggerSignalConfig config)
        : base(config)
    {
        _regex         = new Regex(config.RegexPattern);
        _chatChannel   = config.ChatType;
        _patternConfig = config.PatternConfig;

        Service.ChatGui.ChatMessage += OnChatMessageReceived;
    }

    private void OnChatMessageReceived(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        var channelDescriptor = type.GetChannelDescriptor();

        Service.PluginLog.Verbose("Chat message received.\nTrigger: {0}\n{1}", Name, channelDescriptor);
        if (channelDescriptor.Channel != _chatChannel)
            return;
        try{
            Service.PluginLog.Verbose("Evaluating string regex [{0}] against [{1}]", _regex, message.TextValue);
            var match = _regex.Match(message.TextValue);
            if (!match.Success)
                return;
            Service.PluginLog.Debug("Regex match found, triggering pattern.\nTrigger: {0}", Name);
            TriggerPattern();
        } catch (Exception e){
            Service.PluginLog.Error(e, "Exception while matching regex");
        }
    }

    public override SignalSourceConfig CreateConfig() =>
        new ChatTriggerSignalConfig {
            Name          = Name,
            RegexPattern  = _regex.ToString(),
            ChatType      = _chatChannel,
            PatternConfig = _patternConfig,
        };

    protected override void Dispose(bool disposing)
    {
        Service.ChatGui.ChatMessage -= OnChatMessageReceived;
        base.Dispose(disposing);
    }

    public override void Update(double elapsedMilliseconds)
    {
        var output = SignalOutput.Zero;
        if (_currentPattern is not null)
            output = _currentPattern.Update(elapsedMilliseconds);
        if (_currentPattern is {
                IsCompleted: true,
            })
            _currentPattern = null;
        Output = output;
    }

    private void TriggerPattern() => _currentPattern = SimplePattern.CreatePatternFromConfig(_patternConfig);
}
