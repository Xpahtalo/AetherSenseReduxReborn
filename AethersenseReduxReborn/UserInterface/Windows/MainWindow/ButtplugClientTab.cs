using AethersenseReduxReborn.Buttplug;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using XpahtaLib.UserInterface.Tabs;

namespace AethersenseReduxReborn.UserInterface.Windows.MainWindow;

public class ButtplugClientTab: TabBase
{
    private ButtplugWrapper ButtplugWrapper { get; }

    public override string TabName => "Buttplug";

    public ButtplugClientTab(ButtplugWrapper buttplugButtplugWrapper) { ButtplugWrapper = buttplugButtplugWrapper; }

    protected override void DrawTab()
    {
        ConnectionStatus();
        ConnectionButtons();
        ImGui.Separator();
        ListDevicesAndActuators();
    }

    private void ConnectionStatus()
    {
        switch (ButtplugWrapper.Connected){
            case true:
                ImGui.Text("Connected to buttplug server.");
                break;
            case false:
                ImGui.Text("Click \"Connect\" to start.");
                break;
        }

        var pluginConfiguration = Service.ConfigurationService.PluginConfiguration;
        if (!ButtplugWrapper.Connected){
            var uri = pluginConfiguration.Address;
            if (ImGui.InputText("Server Address", ref uri, 100)){
                pluginConfiguration.Address = uri;
            }
        }
        ImGui.SameLine();
        if (ImGui.Button("Save")){
            ButtplugWrapper.SaveDevicesToConfiguration();
            Service.ConfigurationService.SaveServerConfiguration(pluginConfiguration);
        }

#if DEBUG
        if (ImGui.Button("Open Config Directory")){
            Service.ConfigurationService.OpenConfigDirectory();
        }
#endif
    }

    private void ConnectionButtons()
    {
        if (ButtplugWrapper.Connected){
            if (ImGui.Button("Disconnect")){
                ButtplugWrapper.Disconnect();
            }
        } else{
            if (ImGui.Button("Connect")){
                ButtplugWrapper.Connect();
            }
        }
    }

    private void ListDevicesAndActuators()
    {
        ImGui.Text("Saved and Connected devices:");

        foreach (var device in ButtplugWrapper.Devices){
            var       color = device.IsConnected ? ImGuiColors.DalamudWhite : ImGuiColors.DalamudGrey;
            using var _     = new ImRaii.Color().Push(ImGuiCol.Text, color);
            ImGui.Text(device.DisplayName);
            foreach (var deviceActuator in device.Actuators){
                ImGui.BulletText(deviceActuator.DisplayAttributes);
                ImGui.Text(deviceActuator.CombineType.ToString());
            }
        }
    }
}
