using AethersenseReduxReborn.Buttplug;
using AethersenseReduxReborn.Configurations;
using AethersenseReduxReborn.Signals;
using AethersenseReduxReborn.UserInterface;
using AethersenseReduxReborn.UserInterface.Windows.MainWindow;
using Dalamud.Game.Command;
using Dalamud.Plugin;

namespace AethersenseReduxReborn;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class Plugin: IDalamudPlugin
{
    private readonly ButtplugWrapper _buttplugWrapper;
    private readonly SignalService   _signalService;
    public           string          Name => "Aethersense Redux Reborn";
    private const    string          CommandName = "/arr";


    public Plugin(DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();
        Service.CommandManager.AddHandler(CommandName,
                                          new CommandInfo(OnShowUI) {
                                              HelpMessage = "Opens the Aether Sense Redux configuration window",
                                          });

        Service.ConfigurationService = new ConfigurationService();
        Service.WindowManager        = new WindowManager();

        _buttplugWrapper = new ButtplugWrapper(Name, Service.ConfigurationService.PluginConfiguration);
        _signalService   = new SignalService(_buttplugWrapper, Service.ConfigurationService.SignalPluginConfiguration);


//        Service.WindowManager.AddWindow(MainWindow.Name, new MainWindow(_buttplugWrapper, _signalService));
        Service.WindowManager.AddWindow(new MainWindow(_buttplugWrapper, _signalService));

        Service.PluginLog.Information(Service.PluginInterface.GetPluginConfigDirectory());

        Service.PluginInterface.UiBuilder.Draw         += DrawUi;
        Service.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUi;
    }


    public void Dispose()
    {
        Service.CommandManager.RemoveHandler(CommandName);
        _buttplugWrapper.Dispose();
    }

#region UI Handlers

    private static void OnShowUI(string command, string args) => Service.WindowManager.ToggleWindow(MainWindow.Name);

    private static void DrawUi() => Service.WindowManager.Draw();

    private static void DrawConfigUi() => Service.WindowManager.ToggleWindow(MainWindow.Name);

#endregion
}
