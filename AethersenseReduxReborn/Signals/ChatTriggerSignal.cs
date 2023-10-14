using System;
using System.Text.RegularExpressions;
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
        var channel = XIVChatTypeEx.Decode((uint)type).Item3;

        if (channel != _chatChannel)
            return;
        try{
            Service.PluginLog.Debug("Evaluating string regex [{0}] against [{1}]", _regex, message.TextValue);
            var match = _regex.Match(message.TextValue);
            if (!match.Success)
                return;
            TriggerPattern();
        } catch (Exception e){
            Service.PluginLog.Error(e, "Error while matching regex");
        }
    }

    protected override void Dispose(bool disposing)
    {
        Service.ChatGui.ChatMessage -= OnChatMessageReceived;
        base.Dispose(disposing);
    }

    public override void Update(double elapsedMilliseconds)
    {
        var output = 0.0d;
        if (_currentPattern is not null)
            output = _currentPattern.Update(elapsedMilliseconds);
        if (_currentPattern is {
                IsCompleted: true,
            })
            _currentPattern = null;
        Value = output;
    }

    private void TriggerPattern() => _currentPattern = SimplePattern.CreatePatternFromConfig(_patternConfig);
}

public class ChatTriggerSignalConfig: SignalSourceConfig
{
    public required string              RegexPattern  { get; set; }
    public required Channel             ChatType      { get; set; }
    public required SimplePatternConfig PatternConfig { get; set; }

    public static ChatTriggerSignalConfig DefaultConfig() =>
        new() {
            PatternConfig = SimplePatternConfig.DefaultConstantPattern(),
            RegexPattern  = "",
            ChatType      = Channel.BattleSystemMessage,
        };
}
