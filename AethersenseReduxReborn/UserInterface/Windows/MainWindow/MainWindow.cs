using System;
using System.Collections.Generic;
using AethersenseReduxReborn.Signals;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using XpahtaLib.UserInterface.Tabs;

namespace AethersenseReduxReborn.UserInterface.Windows.MainWindow;

public sealed class MainWindow: Window, IDisposable
{
    private TabBar TabBar { get; }

    public static string Name => "Aethersense Redux Reborn - This is in super alpha don't expect it to work.";

    public MainWindow(SignalService signalService)
        : base(Name)
    {
        TabBar = new TabBar {
            Name = "Main Window Tab Bar",
            Tabs = new List<TabBase> {
                new ButtplugClientTab(signalService.ButtplugWrapper),
                new SignalGroupTab(signalService),
            },
        };
    }

    public override void Draw()
    {
        ImGui.Text("This UI is considered very temporary and everything is likely to change. Feel free to give feedback on the discord.");
        TabBar.Draw();
    }

    public override void OnOpen()
    {
        base.OnOpen();
        TabBar.OnOpen();
    }

    public override void OnClose()
    {
        base.OnClose();
        TabBar.OnClose();
    }

    public void Dispose() { TabBar.Dispose(); }
}
