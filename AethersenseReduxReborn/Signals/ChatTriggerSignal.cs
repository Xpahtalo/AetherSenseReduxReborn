using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using XIVChatTools;

namespace AethersenseReduxReborn.Signals;

public class ChatTriggerSignal: SignalBase
{
    private          SimplePattern?          _currentPattern;
    private readonly ChatTriggerSignalConfig _config;

    public ChatTriggerSignal(ChatTriggerSignalConfig config)
    {
        _config                     =  config;
        Service.ChatGui.ChatMessage += OnChatMessageReceived;
    }

    private void OnChatMessageReceived(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        var channel = XIVChatTypeEx.Decode((uint)type).Item3;

        if (channel != _config.ChatType)
            return;
        try{
            Service.PluginLog.Debug("Evaluating string regex [{0}] against [{1}]", _config.Regex, message.TextValue);
            var match = _config.Regex.Match(message.TextValue);
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

    private void TriggerPattern() => _currentPattern = SimplePattern.CreatePatternFromConfig(_config.PatternConfig);
}

public class ChatTriggerSignalConfig: SignalSourceConfig
{
    public required SimplePatternConfig PatternConfig { get; set; }
    [JsonConverter(typeof(RegexConverter))]
    public required Regex Regex { get;      set; }
    public required Channel ChatType { get; set; }

    public static ChatTriggerSignalConfig DefaultConfig() =>
        new() {
            PatternConfig = SimplePatternConfig.DefaultConstantPattern(),
            Regex         = new Regex(""),
            ChatType      = Channel.BattleSystemMessage,
        };
}

public class RegexConverter: JsonConverter<Regex>
{
    public override Regex Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => new(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, Regex value, JsonSerializerOptions options) { writer.WriteStringValue(value.ToString()); }
}
