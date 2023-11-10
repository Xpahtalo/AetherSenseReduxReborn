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
    private bool Editing { get; set; } = true;
    private string TextToTest {
        get => _textToTest;
        set {
            _textToTest      = value;
            TextToTestAsList = SplitAtNewLines(_textToTest);
        }
    }
    private string RegexPattern {
        get => _regexPattern;
        set {
            _regexPattern = value;
            try{
                Regex = new Regex(_regexPattern);
            } catch{
                Regex = null;
            }
        }
    }
    private Regex?              Regex            { get; set; } = new(string.Empty);
    private bool                RegexIsValid     => Regex is not null;
    private IEnumerable<string> TextToTestAsList { get; set; } = Array.Empty<string>();

    private string _regexPattern = string.Empty;
    private string _textToTest   = string.Empty;

    private readonly float              _yForStaticElements = ImGui.GetTextLineHeightWithSpacing() * 8;
    private readonly Button             _editButton;
    private readonly MultiLineTextInput _testInput;
    private readonly TextInput          _regexInput;
    private readonly Button             _confirmButton;

    private const ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.AlwaysAutoResize;

    public RegexHelper(string name, Action<string> confirmed, bool forceMainWindow = false)
        : base(name, WindowFlags, forceMainWindow)
    {
        _editButton = new Button("Edit",
                                 () => Editing = !Editing);
        _testInput = new MultiLineTextInput("",
                                            text => TextToTest = text,
                                            2000,
                                            cleanClipboard: true);

        _regexInput = new TextInput("",
                                    2000,
                                    text => RegexPattern = text);

        _confirmButton = new Button("Confirm",
                                    () =>
                                    {
                                        confirmed.Invoke(RegexPattern);
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
        if (Editing){
            DrawEditing();
        } else{
            DrawTesting();
        }
        ImGui.Separator();
        DrawRegexSection();
        _confirmButton.Draw();
    }

    private void DrawEditing()
    {
        ImGui.Text("""
                   Enter a list of strings you would like to test against, seperated by newlines.
                   When you are done, Click "Done" to change to testing mode.
                   """);
        _editButton.Draw("Done");
        _testInput.Draw(TextToTest);
    }

    private void DrawTesting()
    {
        ImGui.Text("""
                   Lines that match the Regex will be colored green, and those that don't will be red.
                   Click "Edit" to change back to editing mode.
                   """);
        _editButton.Draw("Edit");
        foreach (var line in TextToTestAsList){
            var color = Regex is not null
                            ? Regex.IsMatch(line)
                                  ? ImGuiColors.HealerGreen
                                  : ImGuiColors.DPSRed
                            : ImGuiColors.DalamudWhite;
            using var style = ImRaii.PushColor(ImGuiCol.Text, color);
            ImGui.Text(line);
        }
    }

    private void DrawRegexSection()
    {
        ImGui.Text("Enter your regex below.");
        _regexInput.Draw(RegexPattern);
        if (!RegexIsValid){
            ImGui.SameLine();
            ImGui.TextColored(ImGuiColors.DPSRed, "(!)");
            if (ImGui.IsItemHovered()){
                ImGui.SetTooltip("Invalid Regex");
            }
        }
    }

    public override void OnClose()
    {
        base.OnClose();
        Service.WindowManager.RemoveWindow(this);
    }

    private static IEnumerable<string> SplitAtNewLines(string text)
    {
        using var reader = new StringReader(text);
        var       line   = reader.ReadLine();
        while (line is not null){
            yield return line;
            line = reader.ReadLine();
        }
    }
}
