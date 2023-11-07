using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dalamud.Interface.Windowing;

namespace AethersenseReduxReborn.UserInterface;

public sealed class WindowManager: IDisposable
{
    private readonly WindowSystem _windowSystem = new("Aethersense Redux Reborn");

    private bool GetWindowByName(string name, [NotNullWhen(true)] out Window? window)
    {
        var matching = from storedWindow in _windowSystem.Windows
                       where storedWindow.WindowName == name
                       select storedWindow;
        var enumerable = matching.ToList();
        switch (enumerable.Count){
            case > 1:
                Service.PluginLog.Error("Found multiple windows with name {0}", name);
                window = null;
                return false;
            case 0:
                window = null;
                return false;
            default:
                window = enumerable.First();
                return true;
        }
    }

    public bool AddWindow(Window window)
    {
        if (GetWindowByName(window.WindowName, out _)){
            Service.PluginLog.Error("Window with name {0} already exists. Cannot add.", window.WindowName);
            return false;
        }
        Service.PluginLog.Information("Adding window {0}", window.WindowName);
        _windowSystem.AddWindow(window);
        return true;
    }

    public bool RemoveWindow(string name)
    {
        if (!GetWindowByName(name, out var window)){
            Service.PluginLog.Error("Unable to find window with name {0} to remove.", name);
            return false;
        }
        Service.PluginLog.Information("Removing window {0}", name);
        _windowSystem.RemoveWindow(window);
        var disposable = window as IDisposable;
        disposable?.Dispose();
        return true;
    }

    public bool RemoveWindow(Window window) => RemoveWindow(window.WindowName);

    public bool OpenWindow(string name)
    {
        if (!GetWindowByName(name, out var window)){
            Service.PluginLog.Error("Unable to find window with name {0} to open.", name);
            return false;
        }
        Service.PluginLog.Information("Opening window {0}", name);
        window.IsOpen = true;
        window.BringToFront();
        return true;
    }

    public bool CloseWindow(string name)
    {
        if (!GetWindowByName(name, out var window)){
            Service.PluginLog.Error("Unable to find window with name {0} to close.", name);
            return false;
        }
        Service.PluginLog.Information("Closing window {0}", name);
        window.IsOpen = false;
        return true;
    }

    public bool ToggleWindow(string name)
    {
        if (!GetWindowByName(name, out var window)){
            Service.PluginLog.Error("Unable to find window with name {0} to toggle.", name);
            return false;
        }
        Service.PluginLog.Information("Toggling window {0}", name);
        window.IsOpen = !window.IsOpen;
        return true;
    }

    public void Draw() => _windowSystem.Draw();

    public void Dispose()
    {
        var toDispose =
            from window in _windowSystem.Windows
            where window is IDisposable
            select window as IDisposable;
        _windowSystem.RemoveAllWindows();
        toDispose.ToList().ForEach(disposable => disposable.Dispose());
    }
}
