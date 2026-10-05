using AccessKit;
using AccessKit.Input;
using AccessKit.Speech;
using AccessKit.Text;
using Il2CppInterop.Runtime;
using Mimick.UI;
using MimicPartyAccess.Game.Announcements;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MimicPartyAccess.Game;

/// <summary>S during a match reads the round and every player's total score.</summary>
public static class ScoreHotkey
{
    public static void Tick()
    {
        if (!ModConfig.GameAnnouncements || !KeyInput.Down(KeyCode.S) || KeyInput.AnyModifierHeld || IsTyping())
            return;

        var screen = ActiveGameScreen();
        if (screen == null)
            return;

        var round = screen.roundLabel == null ? "" : TextCleaner.Clean(screen.roundLabel.text);
        var standings = RoundStandings.DescribeAll(screen.roundController) ?? Strings.Get("game.score.unavailable");
        ScreenReader.Say(round.Length > 0 ? $"{round}. {standings}" : standings, interrupt: true);
    }

    private static bool IsTyping()
    {
        var selected = EventSystem.current?.currentSelectedGameObject;
        return selected != null && selected.GetComponent<TMP_InputField>() is { isFocused: true };
    }

    private static GameScreen? ActiveGameScreen()
    {
        foreach (var obj in UnityEngine.Object.FindObjectsByType(Il2CppType.Of<GameScreen>(), FindObjectsSortMode.None))
        {
            var screen = obj.TryCast<GameScreen>();
            if (screen != null && screen.isActiveAndEnabled)
                return screen;
        }
        return null;
    }
}
