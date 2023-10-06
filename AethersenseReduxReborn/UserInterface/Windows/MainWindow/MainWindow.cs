using System;
using System.Collections.Generic;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using XpahtaLib.UserInterface.Tabs;

namespace AethersenseReduxReborn.UserInterface.Windows.MainWindow;

public sealed class MainWindow: Window, IDisposable
{
    private readonly TabBar _tabBar;
    
    public static string Name => "Aethersense Redux Reborn";

    public MainWindow(ButtplugWrapper buttplugWrapper, SignalService signalService)
        : base(Name)
    {
        _tabBar = new TabBar {
                                   Name= "Main Window Tab Bar",
                                   Tabs= new List<TabBase> {
                                                             new ButtplugClientTab(buttplugWrapper),
                                                             new SignalGroupTab(buttplugWrapper, signalService),
                                                         },
                               };
    }

    public override void Draw()
    {
        _tabBar.Draw();
    }

    public void Dispose()
    {
        _tabBar.Dispose();
    }
}
