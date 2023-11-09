using System;
using System.Collections.Generic;
using AethersenseReduxReborn.Buttplug;
using AethersenseReduxReborn.Signals;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using XpahtaLib.UserInterface.Tabs;

namespace AethersenseReduxReborn.UserInterface.Windows.MainWindow;

public sealed class MainWindow: Window, IDisposable
{
    private readonly TabBar _tabBar;

    public static string Name => "Aethersense Redux Reborn - This is in super alpha don't expect it to work.";

    public MainWindow(ButtplugWrapper buttplugWrapper, SignalService signalService)
        : base(Name)
    {
        _tabBar = new TabBar {
            Name = "Main Window Tab Bar",
            Tabs = new List<TabBase> {
                new ButtplugClientTab(buttplugWrapper),
                new SignalGroupTab(buttplugWrapper, signalService),
            },
        };
    }

    public override void Draw()
    {
        ImGui.Text("This UI is considered very temporary and everything is likely to change. Feel free to give feedback on the discord.");
        _tabBar.Draw();
    }

    public override void OnOpen()
    {
        base.OnOpen();
        _tabBar.OnOpen();
    }

    public override void OnClose()
    {
        base.OnClose();
        _tabBar.OnClose();
    }

    public void Dispose() { _tabBar.Dispose(); }
}
