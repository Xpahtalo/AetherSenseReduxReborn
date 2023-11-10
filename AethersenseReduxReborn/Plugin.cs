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
    private       SignalService SignalService { get; }
    public static string        Name          => "Aethersense Redux Reborn";
    private const string        CommandName = "/arr";

    public Plugin(DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();
        Service.CommandManager.AddHandler(CommandName,
                                          new CommandInfo(OnShowUI) {
                                              HelpMessage = "Opens the Aether Sense Redux configuration window",
                                          });

        Service.ConfigurationService = new ConfigurationService();
        Service.WindowManager        = new WindowManager();
        SignalService                = new SignalService();

        Service.WindowManager.AddWindow(new MainWindow(SignalService));

        Service.PluginInterface.UiBuilder.Draw         += DrawUi;
        Service.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUi;
    }


    public void Dispose()
    {
        SignalService.Dispose();
        Service.CommandManager.RemoveHandler(CommandName);
    }

    private static void OnShowUI(string command, string args) => Service.WindowManager.ToggleWindow(MainWindow.Name);

    private static void DrawUi() => Service.WindowManager.Draw();

    private static void DrawConfigUi() => Service.WindowManager.ToggleWindow(MainWindow.Name);
}
