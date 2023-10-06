using System.IO;
using System.Text.Json;

namespace AethersenseReduxReborn.Configurations;

public class ConfigurationService
{
    private readonly string _configDirectory; 
    
    public ButtplugServerConfiguration ServerConfiguration { get; private set; }
    public SignalConfiguration         SignalConfiguration { get; private set; }
    
    
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
        if (!File.Exists(path))
        {
            ServerConfiguration = new ButtplugServerConfiguration();
            return;
        }

        var json = File.ReadAllText(path);
        ServerConfiguration = JsonSerializer.Deserialize<ButtplugServerConfiguration>(json);
    }
    
    public void LoadSignalConfiguration()
    {
        Service.PluginLog.Information("Loading signal configuration.");
        var path = Path.Combine(_configDirectory, "signal.json");
        if (!File.Exists(path))
        {
            SignalConfiguration = SignalConfiguration.DefaultConfiguration();
            return;
        }
        
        var json = File.ReadAllText(path);
        SignalConfiguration = JsonSerializer.Deserialize<SignalConfiguration>(json);
    }
    
    public void SaveServerConfiguration(ButtplugServerConfiguration serverConfiguration)
    {
        Service.PluginLog.Information("Saving server configuration.");
        ServerConfiguration = serverConfiguration;
        var json = JsonSerializer.Serialize(ServerConfiguration);
        File.WriteAllText(Path.Combine(_configDirectory, "server.json"), json);
    }

    public void SaveSignalConfiguration(SignalConfiguration signalConfiguration)
    {
        Service.PluginLog.Information("Saving signal configuration.");
        SignalConfiguration = signalConfiguration;
        var json = JsonSerializer.Serialize(SignalConfiguration);
        File.WriteAllText(Path.Combine(_configDirectory, "signal.json"), json);
    }
}
