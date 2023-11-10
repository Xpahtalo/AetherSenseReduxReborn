using System;
using System.Text.RegularExpressions;
using AethersenseReduxReborn.Signals.Configs;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using XIVChatTools;

namespace AethersenseReduxReborn.Signals;

public class ChatTriggerSignal: SignalBase
{
    private SimplePattern?      CurrentPattern { get; set; }
    private Regex               Regex          { get; }
    private Channel             ChatChannel    { get; }
    private SimplePatternConfig PatternConfig  { get; }

    public ChatTriggerSignal(ChatTriggerSignalConfig config)
        : base(config)
    {
        Regex         = new Regex(config.RegexPattern);
        ChatChannel   = config.ChatType;
        PatternConfig = config.PatternConfig;

        Service.ChatGui.ChatMessage += OnChatMessageReceived;
    }

    private void OnChatMessageReceived(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        var channelDescriptor = type.GetChannelDescriptor();

        Service.PluginLog.Verbose("Chat message received.\nTrigger: {0}\n{1}", Name, channelDescriptor);
        if (channelDescriptor.Channel != ChatChannel){
            return;
        }
        try{
            Service.PluginLog.Verbose("Evaluating string regex [{0}] against [{1}]", Regex, message.TextValue);
            var match = Regex.Match(message.TextValue);
            if (!match.Success){
                return;
            }
            Service.PluginLog.Debug("Regex match found, triggering pattern.\nTrigger: {0}", Name);
            TriggerPattern();
        } catch (Exception e){
            Service.PluginLog.Error(e, "Exception while matching regex");
        }
    }

    public override SignalSourceConfig CreateConfig() =>
        new ChatTriggerSignalConfig {
            Name          = Name,
            RegexPattern  = Regex.ToString(),
            ChatType      = ChatChannel,
            PatternConfig = PatternConfig,
        };

    protected override void Dispose(bool disposing)
    {
        Service.ChatGui.ChatMessage -= OnChatMessageReceived;
        base.Dispose(disposing);
    }

    public override void Update(double elapsedMilliseconds)
    {
        var output = SignalOutput.Zero;
        if (CurrentPattern is not null){
            output = CurrentPattern.Update(elapsedMilliseconds);
        }
        if (CurrentPattern is {
                IsCompleted: true,
            }){
            CurrentPattern = null;
        }
        Output = output;
    }

    private void TriggerPattern() => CurrentPattern = new SimplePattern(PatternConfig);
}
