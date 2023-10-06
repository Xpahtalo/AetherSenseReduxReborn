using AethersenseReduxReborn.Configurations;
using AethersenseReduxReborn.UserInterface;
using AethersenseReduxReborn.UserInterface.Windows.MainWindow;
using Dalamud.Game.Command;
using Dalamud.Plugin;

namespace AethersenseReduxReborn;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class Plugin: IDalamudPlugin
{
    private readonly WindowManager               _windowManager;
    private readonly ButtplugWrapper             _buttplugWrapper;
    private readonly SignalService               _signalService;
    public           string                      Name => "Aethersense Redux Reborn";
    private const    string                      CommandName = "/arr";


    public Plugin(DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();
        Service.CommandManager.AddHandler(CommandName,
                                          new CommandInfo(OnShowUI) {
                                                                        HelpMessage = "Opens the Aether Sense Redux configuration window",
                                                                    });
        
        Service.ConfigurationService = new ConfigurationService();

        _buttplugWrapper     = new ButtplugWrapper(Name, Service.ConfigurationService.ServerConfiguration);
        _signalService       = new SignalService(_buttplugWrapper, Service.ConfigurationService.SignalConfiguration);

        _windowManager = new WindowManager();
        _windowManager.AddWindow(MainWindow.Name, new MainWindow(_buttplugWrapper, _signalService));
        
        Service.PluginLog.Information(Service.PluginInterface.GetPluginConfigDirectory());
        
        Service.PluginInterface.UiBuilder.Draw         += DrawUi;
        Service.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUi;
    }


    public void Dispose()
    {
        _buttplugWrapper.Dispose();
        Service.CommandManager.RemoveHandler(CommandName);
    }

#region UI Handlers

    private void OnShowUI(string command, string args) { _windowManager.ToggleWindow(MainWindow.Name); }

    private void DrawUi() { _windowManager.Draw(); }

    private void DrawConfigUi() { _windowManager.ToggleWindow(MainWindow.Name); }

#endregion
}
