using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace AethersenseReduxReborn.Configurations;

public class ConfigurationService
{
    private readonly string _configDirectory;

    public ButtplugPluginConfiguration PluginConfiguration       { get; private set; } = null!;
    public SignalPluginConfiguration   SignalPluginConfiguration { get; private set; } = null!;

    public ConfigurationService()
    {
        _configDirectory = Service.PluginInterface.GetPluginConfigDirectory();
        LoadServerConfiguration();
        LoadSignalConfiguration();
    }

    public void OpenConfigDirectory() { Process.Start("explorer.exe", _configDirectory); }

    public void LoadServerConfiguration()
    {
        Service.PluginLog.Information("Loading server configuration.");
        var path = Path.Combine(_configDirectory, "server.json");
        if (!File.Exists(path)){
            PluginConfiguration = GetDefaultServerConfiguration();
            return;
        }

        var json         = File.ReadAllText(path);
        var loadedConfig = JsonSerializer.Deserialize<ButtplugPluginConfiguration>(json, Json.Options);
        PluginConfiguration = loadedConfig ?? GetDefaultServerConfiguration();
        return;

        static ButtplugPluginConfiguration GetDefaultServerConfiguration() { return new ButtplugPluginConfiguration(); }
    }


    public void LoadSignalConfiguration()
    {
        Service.PluginLog.Information("Loading signal configuration.");
        var path = Path.Combine(_configDirectory, "signal.json");
        if (!File.Exists(path)){
            SignalPluginConfiguration = GetDefaultConfiguration();
            return;
        }

        var json         = File.ReadAllText(path);
        var loadedConfig = JsonSerializer.Deserialize<SignalPluginConfiguration>(json, Json.Options);
        SignalPluginConfiguration = loadedConfig ?? GetDefaultConfiguration();
        return;

        static SignalPluginConfiguration GetDefaultConfiguration() { return SignalPluginConfiguration.GetDefaultConfiguration(); }
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
