using AethersenseReduxReborn.Configurations;
using ImGuiNET;
using XpahtaLib.UserInterface.Tabs;

namespace AethersenseReduxReborn.UserInterface.Windows.MainWindow;

public class ButtplugClientTab: TabBase
{
    private ButtplugServerConfiguration _serverConfiguration;
    
    private readonly ButtplugWrapper _buttplugWrapper;

    public override string Name => "Buttplug";

    public ButtplugClientTab(ButtplugWrapper buttplugWrapper)
    {
        _serverConfiguration = Service.ConfigurationService.ServerConfiguration;
        _buttplugWrapper = buttplugWrapper;
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
            var uri = _serverConfiguration.Address;
            if (ImGui.InputText("Server Address", ref uri, 100))
                _serverConfiguration.Address = uri;
            ImGui.SameLine();
            if (ImGui.Button("Save"))
                Service.ConfigurationService.SaveServerConfiguration(_serverConfiguration);
        }
    }

    private void ConnectionButtons()
    {
        if (_buttplugWrapper.Connected){
            if (ImGui.Button("Disconnect")) _buttplugWrapper.Disconnect().Start();
        } else{
            if (ImGui.Button("Connect")) _buttplugWrapper.Connect().Start();
        }
    }

    private void ListDevicesAndActuators()
    {
        foreach (var device in _buttplugWrapper.Devices){
            ImGui.Text(device.Name);
            ImGui.Indent();
            foreach (var actuator in device.Actuators){
                ImGui.Text($"{actuator.Index} - {actuator.ActuatorType} - {actuator.Description} - {actuator.Steps}");
            }

            ImGui.Unindent();
        }
    }
}
