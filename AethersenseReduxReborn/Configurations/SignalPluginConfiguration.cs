using System.Collections.Generic;
using AethersenseReduxReborn.Signals.Configs;
using XIVChatTools;

namespace AethersenseReduxReborn.Configurations;

public sealed class SignalPluginConfiguration
{
    public          int                            Version              { get; set; } = 1;
    public required List<SignalGroupConfiguration> SignalConfigurations { get; set; }

    public static SignalPluginConfiguration GetDefaultConfiguration() =>
        new() {
            SignalConfigurations = new List<SignalGroupConfiguration> {
                new() {
                    Name        = "New Group",
                    CombineType = CombineType.Max,
                    SignalSources = new List<SignalSourceConfig> {
                        new ChatTriggerSignalConfig {
                            ChatType      = Channel.Action,
                            Name          = "New Trigger",
                            PatternConfig = SimplePatternConfig.DefaultConstantPattern(),
                            RegexPattern  = "cast",
                        },
                    },
                },
            },
        };
}
