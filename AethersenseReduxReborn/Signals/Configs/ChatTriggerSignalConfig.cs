using XIVChatTools;

namespace AethersenseReduxReborn.Signals.Configs;

public class ChatTriggerSignalConfig: SignalSourceConfig
{
    public required string              RegexPattern  { get; set; }
    public required Channel             ChatType      { get; set; }
    public required SimplePatternConfig PatternConfig { get; set; }

    public static ChatTriggerSignalConfig DefaultConfig() =>
        new() {
            PatternConfig = SimplePatternConfig.DefaultConstantPattern(),
            RegexPattern  = "",
            ChatType      = Channel.Action,
        };
}
