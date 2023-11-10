using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AethersenseReduxReborn.Signals;
using AethersenseReduxReborn.Signals.Configs;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using XpahtaLib.UserInterface;
using XpahtaLib.UserInterface.Tabs;

namespace AethersenseReduxReborn.UserInterface.Windows.MainWindow;

public class SignalGroupTab: TabBase
{
    private List<SignalGroupConfiguration> TempSignalGroupConfigurations { get; }
    private SignalGroupConfiguration?      SelectedSignalGroupConfig     { get; set; }

    private SingleSelectionList<SignalGroupConfiguration> SignalGroupList          { get; }
    private Button                                        RemoveSignalGroupButton  { get; }
    private Button                                        AddSignalGroupButton     { get; }
    private Button                                        SaveConfigurationButton  { get; }
    private Button                                        ApplyConfigurationButton { get; }
    private SignalConfigChild?                            ConfigChild              { get; set; }

    public override string TabName => "Signal Groups";

    public SignalGroupTab(SignalService signalService)
    {
        TempSignalGroupConfigurations = signalService.GetSignalGroupConfigurations().ToList();

        SignalGroupList = new SingleSelectionList<SignalGroupConfiguration>("Signal Groups",
                                                                            config => config.Name,
                                                                            (config1, config2) => config1 == config2,
                                                                            selectedConfig =>
                                                                            {
                                                                                SelectedSignalGroupConfig = selectedConfig;
                                                                                if (SelectedSignalGroupConfig is not null){
                                                                                    Service.PluginLog.Debug("SignalGroupTab: Selected SignalGroupConfiguration: {0}", SelectedSignalGroupConfig.Name);
                                                                                    ConfigChild = new SignalConfigChild(SelectedSignalGroupConfig, signalService, signalService.ButtplugWrapper);
                                                                                }
                                                                            });
        RemoveSignalGroupButton = new Button("Remove",
                                             () =>
                                             {
                                                 ConfigChild = null;
                                                 if (SelectedSignalGroupConfig is not null){
                                                     TempSignalGroupConfigurations.Remove(SelectedSignalGroupConfig);
                                                 }
                                             });
        AddSignalGroupButton = new Button("Add",
                                          () =>
                                          {
                                              TempSignalGroupConfigurations.Add(new SignalGroupConfiguration {
                                                  CombineType   = CombineType.Max,
                                                  Name          = "New Signal Group",
                                                  SignalSources = new List<SignalSourceConfig>(),
                                              });
                                              Service.PluginLog.Debug("SignalGroupTab: Add new SignalGroupConfiguration");
                                          });
        SaveConfigurationButton = new Button("Save",
                                             () =>
                                             {
                                                 signalService.ButtplugWrapper.SaveDevicesToConfiguration();
                                                 signalService.SaveConfiguration(TempSignalGroupConfigurations);
                                             });
        ApplyConfigurationButton = new Button("Apply", () => signalService.ApplyConfiguration(TempSignalGroupConfigurations));
    }

    protected override void DrawTab()
    {
        using (var listChild = ImRaii.Child("###GroupListChild", new Vector2(ImGui.GetContentRegionAvail().X * 0.25f, 0), true)){
            if (!listChild){ } else{
                var listRegion = new Vector2 {
                    X = ImGui.GetContentRegionAvail().X,
                    Y = (ImGui.GetContentRegionAvail().Y / ImGui.GetTextLineHeightWithSpacing() - 1) * ImGui.GetTextLineHeightWithSpacing(),
                };
                SignalGroupList.Draw(SelectedSignalGroupConfig, TempSignalGroupConfigurations, listRegion);


                RemoveSignalGroupButton.Draw();
                ImGui.SameLine();
                AddSignalGroupButton.Draw();
                ImGui.SameLine();
                ApplyConfigurationButton.Draw();
                ImGui.SameLine();
                SaveConfigurationButton.Draw();
            }
        }
        ImGui.SameLine();
        ConfigChild?.Draw();
    }
}
