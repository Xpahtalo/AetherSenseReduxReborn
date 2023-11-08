using System.Collections.Generic;
using AethersenseReduxReborn.Signals;
using AethersenseReduxReborn.Signals.Configs;
using XIVChatTools;

namespace AethersenseReduxReborn.Configurations;

public sealed class SignalPluginConfiguration
{
    public int Version { get; set; } = 1;

    public required List<SignalGroupConfiguration> SignalConfigurations { get; set; }

    public static SignalPluginConfiguration GetDefaultConfiguration() =>
        new() {
            SignalConfigurations = new List<SignalGroupConfiguration> {
                SignalDefaults.Caster,
                SignalDefaults.Target,
            },
        };
}

public static class SignalDefaults
{
    private static ChatTriggerSignalConfig CastingConfig => new() {
        Name     = "Casting",
        ChatType = Channel.Action,
        PatternConfig = new SimplePatternConfig {
            PatternType   = SimplePatternType.Ramp,
            TotalDuration = 2000,
            Intensity1    = 0.25,
            Intensity2    = 0.75,
        },
        RegexPattern = "You begin casting",
    };

    private static ChatTriggerSignalConfig CastConfig => new() {
        Name     = "Cast",
        ChatType = Channel.Action,
        PatternConfig = new SimplePatternConfig {
            PatternType   = SimplePatternType.Constant,
            TotalDuration = 250,
            Intensity1    = 1,
        },
        RegexPattern = "You cast",
    };

    private static ChatTriggerSignalConfig UseConfig => new() {
        Name         = "Use",
        RegexPattern = "You use",
        ChatType     = Channel.Action,
        PatternConfig = new SimplePatternConfig {
            PatternType   = SimplePatternType.Constant,
            TotalDuration = 250,
            Intensity1    = 0.90,
        },
    };

    private static CharacterAttributeSignalConfig TargetHealth => new() {
        CharacterName    = "{target}",
        AttributeToTrack = AttributeToTrack.Hp,
        Correlation      = Correlation.Inverse,
        Name             = "Target Health",
    };
    
    public static SignalGroupConfiguration Caster => new() {
        Name        = "Caster",
        CombineType = CombineType.Max,
        SignalSources = new List<SignalSourceConfig> {
            CastingConfig,
            CastConfig,
            UseConfig,
        },
    };

    public static SignalGroupConfiguration Target => new() {
        Name        = "Target",
        CombineType = CombineType.Max,
        SignalSources = new List<SignalSourceConfig> {
            TargetHealth,
        },
    };
}
