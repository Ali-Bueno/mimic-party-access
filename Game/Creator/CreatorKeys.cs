using AccessKit.Input;
using AccessKit.Text;
using AccessKit.Ui;
using Mimick.UI;
using UnityEngine;

namespace MimicPartyAccess.Game.Creator;

/// <summary>
/// Ctrl shortcuts for the pack creator, which the generic menu navigation ignores: Up/Down move a sound row, Left/Right
/// (with Shift: length) nudge the trim selection, I opens the import choices, R reads progress or trim state.
/// </summary>
public static class CreatorKeys
{
    public static void Tick()
    {
        SoundReorder.Tick();
        if (!KeyInput.CtrlHeld || KeyInput.AltHeld || ChoicePrompt.IsOpen)
            return;

        if (KeyInput.Down(KeyCode.UpArrow)) SoundReorder.TryMove(-1);
        else if (KeyInput.Down(KeyCode.DownArrow)) SoundReorder.TryMove(1);
        else if (KeyInput.Down(KeyCode.LeftArrow)) TrimKeys.TryNudge(-1, KeyInput.ShiftHeld);
        else if (KeyInput.Down(KeyCode.RightArrow)) TrimKeys.TryNudge(1, KeyInput.ShiftHeld);
        else if (KeyInput.Down(KeyCode.R)) CreatorReadout.Speak();
        else if (KeyInput.Down(KeyCode.I)) OfferImport();
    }

    private static void OfferImport()
    {
        var screen = CreatorLookup.FindActive<ThemeCreatorScreen>();
        if (screen == null)
            return;
        ChoicePrompt.Show(Strings.Get("creator.import.title"), new (string Label, Action Choose)[]
        {
            (Strings.Get("creator.import.sounds"), () => screen.PickSoundFiles()),
            (Strings.Get("creator.import.image"), () => screen.PickPackImage()),
            (Strings.Get("creator.import.folder"), () => screen.ImportFolder()),
        });
    }
}
