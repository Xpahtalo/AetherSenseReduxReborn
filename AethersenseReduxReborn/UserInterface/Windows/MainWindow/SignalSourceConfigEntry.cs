using AethersenseReduxReborn.Signals;
using AethersenseReduxReborn.Signals.Configs;
using Dalamud.Interface.Utility.Raii;
using XpahtaLib.UserInterface;
using XpahtaLib.UserInterface.Input;

namespace AethersenseReduxReborn.UserInterface.Windows.MainWindow;

internal abstract class SignalSourceConfigEntry: ImGuiWidget
{
    private readonly   TextInput          _nameInput;
    protected readonly SignalService      SignalService;
    public readonly    SignalSourceConfig SignalSourceConfig;

    protected SignalSourceConfigEntry(SignalSourceConfig signalSourceConfig, SignalService signalService)
    {
        SignalSourceConfig = signalSourceConfig;
        SignalService      = signalService;
        _nameInput = new TextInput("Name",
                                   100,
                                   s => signalSourceConfig.Name = s);
    }

    public virtual void Draw()
    {
        using var id = ImRaii.PushId(Id.ToString());
        _nameInput.Draw(SignalSourceConfig.Name);
    }
}
