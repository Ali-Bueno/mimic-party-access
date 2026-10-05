using UnityEngine;
using AccessKit.Input;
using AccessKit.Speech;

namespace AccessKit.Ui;

/// <summary>
/// A list of choices the mod presents itself when the game asks for one outside its menus (e.g. clicking a
/// character in the 3D scene). While open it owns the menu keys; the game closes it, or a choice does.
/// </summary>
public static class ChoicePrompt
{
    private static List<(string Label, Action Choose)>? _choices;
    private static int _index;

    public static bool IsOpen => _choices != null;

    public static void Show(string title, IEnumerable<(string Label, Action Choose)> choices)
    {
        var list = choices.ToList();
        if (list.Count == 0)
            return;
        _choices = list;
        _index = 0;
        ScreenReader.Say($"{title.TrimEnd('.', ' ')}. {list[0].Label}", interrupt: true);
    }

    public static void Close() => _choices = null;

    /// <summary>Handles the menu keys while the prompt is open; returns false when there is no prompt.</summary>
    public static bool Tick()
    {
        var choices = _choices;
        if (choices == null)
            return false;
        if (KeyInput.CtrlHeld || KeyInput.AltHeld)
            return true;

        if (KeyInput.Down(KeyCode.UpArrow))
            Select((_index - 1 + choices.Count) % choices.Count);
        else if (KeyInput.Down(KeyCode.DownArrow))
            Select((_index + 1) % choices.Count);
        else if (KeyInput.Down(KeyCode.Home))
            Select(0);
        else if (KeyInput.Down(KeyCode.End))
            Select(choices.Count - 1);
        else if (KeyInput.Down(KeyCode.Return) || KeyInput.Down(KeyCode.KeypadEnter))
            Choose(choices[_index]);
        return true;
    }

    private static void Select(int index)
    {
        _index = index;
        ScreenReader.Say(_choices![index].Label, interrupt: true);
    }

    private static void Choose((string Label, Action Choose) choice)
    {
        Close();
        try
        {
            choice.Choose();
        }
        catch (Exception exception)
        {
            ModLog.Warning($"Choice '{choice.Label}' failed: {exception.Message}");
        }
    }
}
