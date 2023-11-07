using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AethersenseReduxReborn.Buttplug;
using AethersenseReduxReborn.Signals;
using AethersenseReduxReborn.Signals.Configs;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using XpahtaLib.UserInterface;
using XpahtaLib.UserInterface.Tabs;

namespace AethersenseReduxReborn.UserInterface.Windows.MainWindow;

public class SignalGroupTab: TabBase
{
    private readonly List<SignalGroupConfiguration> _tempSignalGroupConfigurations;

    private SignalGroupConfiguration? _selectedSignalGroupConfig;

    private readonly SingleSelectionList<SignalGroupConfiguration> _signalGroupList;
    private readonly Button                                        _removeSignalGroupButton;
    private readonly Button                                        _addSignalGroupButton;
    private readonly Button                                        _saveConfigurationButton;
    private readonly Button                                        _applyConfigurationButton;
    private          SignalConfigChild?                            _signalConfigChild;

    public override string Name => "Signal Groups";

    public SignalGroupTab(ButtplugWrapper buttplugWrapper, SignalService signalService)
    {
        var buttplugWrapper1 = buttplugWrapper;
        var signalService1   = signalService;
        _tempSignalGroupConfigurations = signalService.GetSignalGroupConfigurations().ToList();

        _signalGroupList = new SingleSelectionList<SignalGroupConfiguration>("Signal Groups",
                                                                             config => config.Name,
                                                                             (config1, config2) => config1 == config2,
                                                                             selectedConfig =>
                                                                             {
                                                                                 _selectedSignalGroupConfig = selectedConfig;
                                                                                 if (_selectedSignalGroupConfig is not null){
                                                                                     Service.PluginLog.Debug("SignalGroupTab: Selected SignalGroupConfiguration: {0}", _selectedSignalGroupConfig.Name);
                                                                                     _signalConfigChild = new SignalConfigChild(_selectedSignalGroupConfig, signalService1, buttplugWrapper1);
                                                                                 }
                                                                             });
        _removeSignalGroupButton = new Button("Remove",
                                              () =>
                                              {
                                                  _signalConfigChild = null;
                                                  if (_selectedSignalGroupConfig is not null)
                                                      _tempSignalGroupConfigurations.Remove(_selectedSignalGroupConfig);
                                              });
        _addSignalGroupButton = new Button("Add",
                                           () =>
                                           {
                                               _tempSignalGroupConfigurations.Add(new SignalGroupConfiguration {
                                                   CombineType   = CombineType.Max,
                                                   Name          = "New Signal Group",
                                                   SignalSources = new List<SignalSourceConfig>(),
                                               });
                                               Service.PluginLog.Debug("SignalGroupTab: Add new SignalGroupConfiguration");
                                           });
        _saveConfigurationButton = new Button("Save",
                                              () =>
                                              {
                                                  buttplugWrapper1.SaveDevicesToConfiguration();
                                                  signalService1.SaveConfiguration(_tempSignalGroupConfigurations);
                                              });
        _applyConfigurationButton = new Button("Apply", () => signalService1.ApplyConfiguration(_tempSignalGroupConfigurations));
    }

    protected override void DrawTab()
    {
        var availableRegion = ImGui.GetContentRegionAvail();
        DrawSignalGroupList();
        ImGui.SameLine();
        DrawSelectedGroup();
        return;

        void DrawSignalGroupList()
        {
            using var listChild = ImRaii.Child("###GroupListChild", new Vector2(availableRegion.X * 0.25f, 0), true);
            if (!listChild)
                return;

            var listRegion = new Vector2 {
                X = ImGui.GetContentRegionAvail().X,
                Y = (ImGui.GetContentRegionAvail().Y / ImGui.GetTextLineHeightWithSpacing() - 1) * ImGui.GetTextLineHeightWithSpacing(),
            };
            _signalGroupList.Draw(_selectedSignalGroupConfig, _tempSignalGroupConfigurations, listRegion);


            _removeSignalGroupButton.Draw();
            ImGui.SameLine();
            _addSignalGroupButton.Draw();
            ImGui.SameLine();
            _applyConfigurationButton.Draw();
            ImGui.SameLine();
            _saveConfigurationButton.Draw();
        }

        void DrawSelectedGroup() { _signalConfigChild?.Draw(); }
    }
}
