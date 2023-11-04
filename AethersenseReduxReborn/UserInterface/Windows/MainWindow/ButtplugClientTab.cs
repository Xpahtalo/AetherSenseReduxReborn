using AethersenseReduxReborn.Buttplug;
using AethersenseReduxReborn.Configurations;
using ImGuiNET;
using XpahtaLib.UserInterface.Tabs;

namespace AethersenseReduxReborn.UserInterface.Windows.MainWindow;

public class ButtplugClientTab: TabBase
{
    private readonly ButtplugPluginConfiguration _pluginConfiguration;

    private readonly ButtplugWrapper _buttplugWrapper;

    public override string Name => "Buttplug";

    public ButtplugClientTab(ButtplugWrapper buttplugWrapper)
    {
        _pluginConfiguration = Service.ConfigurationService.PluginConfiguration;
        _buttplugWrapper     = buttplugWrapper;
    }

    protected override void DrawTab()
    {
        ConnectionStatus();
        ConnectionButtons();
        ImGui.Separator();
        ListDevicesAndActuators();
    }

    private void ConnectionStatus()
    {
        switch (_buttplugWrapper.Connected){
            case true:
                ImGui.Text("Connected to buttplug server.");
                break;
            case false:
                ImGui.Text("Click \"Connect\" to start.");
                break;
        }

        if (!_buttplugWrapper.Connected){
            var uri = _pluginConfiguration.Address;
            if (ImGui.InputText("Server Address", ref uri, 100))
                _pluginConfiguration.Address = uri;
            ImGui.SameLine();
            if (ImGui.Button("Save")){
                _buttplugWrapper.SaveDevicesToConfiguration();
                Service.ConfigurationService.SaveServerConfiguration(_pluginConfiguration);
            }
        }
    }

    private void ConnectionButtons()
    {
        if (_buttplugWrapper.Connected){
            if (ImGui.Button("Disconnect")) _buttplugWrapper.Disconnect();
        } else{
            if (ImGui.Button("Connect")) _buttplugWrapper.Connect();
        }
    }

    private void ListDevicesAndActuators()
    {
        foreach (var actuator in _buttplugWrapper.ConnectedActuators){
            ImGui.Text(actuator.DisplayName);
        }
    }
}
