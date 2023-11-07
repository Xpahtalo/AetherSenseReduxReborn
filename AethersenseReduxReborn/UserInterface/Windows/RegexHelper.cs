using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.RegularExpressions;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using XpahtaLib.UserInterface;
using XpahtaLib.UserInterface.Input;

namespace AethersenseReduxReborn.UserInterface.Windows;

public class RegexHelper: Window
{
    private bool   _editing   = true;
    private string _regexText = string.Empty;
    private Regex? _regex     = new(string.Empty);
    private string RegexText {
        get => _regexText;
        set {
            _regexText = value;
            try{
                _regex = new Regex(_regexText);
            } catch{
                _regex = null;
            }
        }
    }
    private bool RegexIsValid => _regex is not null;

    private string              _testText = string.Empty;
    private IEnumerable<string> _testLines;
    private string TestText {
        get => _testText;
        set {
            _testText  = value;
            _testLines = SplitAtNewLines(_testText);
        }
    }

    private readonly float _yForStaticElements = ImGui.GetTextLineHeightWithSpacing() * 8;

    private readonly Button             _editButton;
    private readonly MultiLineTextInput _testInput;
    private readonly TextInput          _regexInput;
    private readonly Button             _confirmButton;

    private const ImGuiWindowFlags _flags = ImGuiWindowFlags.AlwaysAutoResize;

    public RegexHelper(string name, Action<string> confirmed, bool forceMainWindow = false)
        : base(name, _flags, forceMainWindow)
    {
        _editButton = new Button("Edit",
                                 () => _editing = !_editing);
        _testInput = new MultiLineTextInput("",
                                            text => TestText = text,
                                            2000,
                                            cleanClipboard: true);

        _regexInput = new TextInput("",
                                    2000,
                                    text => RegexText = text);

        _confirmButton = new Button("Confirm",
                                    () =>
                                    {
                                        confirmed.Invoke(RegexText);
                                        Service.WindowManager.RemoveWindow(this);
                                    });

        SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2 {
                X = 500,
                Y = _yForStaticElements,
            },
            MaximumSize = new Vector2 {
                X = float.MaxValue,
                Y = float.MaxValue,
            },
        };
    }

    public override void Draw()
    {
        if (_editing)
            DrawEditing();
        else
            DrawTesting();
        ImGui.Separator();

        ImGui.Text("Enter your regex below.");
        _regexInput.Draw(RegexText);
        if (!RegexIsValid){
            ImGui.SameLine();
            ImGui.TextColored(ImGuiColors.DPSRed, "(!)");
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Invalid Regex");
        }
        _confirmButton.Draw();
    }

    private void DrawEditing()
    {
        ImGui.Text("""
                   Enter a list of strings you would like to test against, seperated by newlines.
                   When you are done, Click "Done" to change to testing mode.
                   """);
        _editButton.Draw("Done");
        _testInput.Draw(TestText);
    }

    private void DrawTesting()
    {
        ImGui.Text("""
                   Lines that match the Regex will be colored green, and those that don't will be red.
                   Click "Edit" to change back to editing mode.
                   """);
        _editButton.Draw("Edit");
        foreach (var line in _testLines){
            var color = _regex is not null
                            ? _regex.IsMatch(line)
                                  ? ImGuiColors.HealerGreen
                                  : ImGuiColors.DPSRed
                            : ImGuiColors.DalamudWhite;
            using var style = ImRaii.PushColor(ImGuiCol.Text, color);
            ImGui.Text(line);
        }
    }

    public override void OnClose()
    {
        base.OnClose();
        Service.WindowManager.RemoveWindow(this);
    }

    private IEnumerable<string> SplitAtNewLines(string text)
    {
        using var reader = new StringReader(text);
        var       line   = reader.ReadLine();
        while (line is not null){
            yield return line;
            line = reader.ReadLine();
        }
    }
}
