using System.IO;
using System.Text.Json;

namespace AethersenseReduxReborn.Configurations;

public class ConfigurationService
{
    private readonly string _configDirectory;

    public ButtplugPluginConfiguration PluginConfiguration       { get; private set; }
    public SignalPluginConfiguration   SignalPluginConfiguration { get; private set; }

    public ConfigurationService()
    {
        _configDirectory = Service.PluginInterface.GetPluginConfigDirectory();
        LoadServerconfiguration();
        LoadSignalConfiguration();
    }

    public void LoadServerconfiguration()
    {
        Service.PluginLog.Information("Loading server configuration.");
        var path = Path.Combine(_configDirectory, "server.json");
        if (!File.Exists(path)){
            PluginConfiguration = new ButtplugPluginConfiguration();
            return;
        }

        var json = File.ReadAllText(path);
        PluginConfiguration = JsonSerializer.Deserialize<ButtplugPluginConfiguration>(json, Json.Options);
    }

    public void LoadSignalConfiguration()
    {
        Service.PluginLog.Information("Loading signal configuration.");
        var path = Path.Combine(_configDirectory, "signal.json");
        if (!File.Exists(path)){
            SignalPluginConfiguration = SignalPluginConfiguration.GetDefaultConfiguration();
            return;
        }

        var json = File.ReadAllText(path);
        SignalPluginConfiguration = JsonSerializer.Deserialize<SignalPluginConfiguration>(json, Json.Options);
    }

    public void SaveServerConfiguration(ButtplugPluginConfiguration pluginConfiguration)
    {
        Service.PluginLog.Information("Saving server configuration.");
        PluginConfiguration = pluginConfiguration;
        var json = JsonSerializer.Serialize(PluginConfiguration, Json.Options);
        File.WriteAllText(Path.Combine(_configDirectory, "server.json"), json);
    }

    public void SaveSignalConfiguration(SignalPluginConfiguration signalPluginConfiguration)
    {
        Service.PluginLog.Information("Saving signal configuration.");
        SignalPluginConfiguration = signalPluginConfiguration;
        var json = JsonSerializer.Serialize(SignalPluginConfiguration, Json.Options);
        File.WriteAllText(Path.Combine(_configDirectory, "signal.json"), json);
    }
}
