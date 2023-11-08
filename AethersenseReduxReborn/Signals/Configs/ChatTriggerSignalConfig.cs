using XIVChatTools;

namespace AethersenseReduxReborn.Signals.Configs;

public class ChatTriggerSignalConfig: SignalSourceConfig
{
    public required string              RegexPattern  { get; set; }
    public required Channel             ChatType      { get; set; }
    public required SimplePatternConfig PatternConfig { get; set; }

    public static ChatTriggerSignalConfig EmptyConfig() =>
        new() {
            Name         = "",
            RegexPattern = "",
            ChatType     = Channel.Action,
            PatternConfig = new SimplePatternConfig {
                PatternType   = SimplePatternType.Constant,
                TotalDuration = 1000,
            },
        };
}
