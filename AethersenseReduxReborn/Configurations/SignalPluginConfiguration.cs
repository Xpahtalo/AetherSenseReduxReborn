using System.Collections.Generic;
using System.Text.Json;
using AethersenseReduxReborn.Signals;
using AethersenseReduxReborn.Signals.SignalGroup;
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
                            ChatType      = Channel.BattleSystemMessage,
                            Name          = "New Trigger",
                            PatternConfig = SimplePatternConfig.DefaultConstantPattern(),
                            RegexPattern  = "cast",
                        },
                    },
                },
            },
        };

    public SignalPluginConfiguration DeepCopy()
    {
        var config = JsonSerializer.Deserialize<SignalPluginConfiguration>(JsonSerializer.Serialize(this, Json.Options), Json.Options) ?? GetDefaultConfiguration();
        Service.PluginLog.Debug(JsonSerializer.Serialize(config, Json.Options));
        return config;
    }
}
