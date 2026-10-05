using UnityEngine;

namespace AccessKit.Input;

/// <summary>Keyboard state through Unity's legacy Input manager (the game enables it alongside the Input System).</summary>
public static class KeyInput
{
    private static bool _unavailable;

    public static bool Down(KeyCode key) => Query(() => UnityEngine.Input.GetKeyDown(key));
    public static bool Held(KeyCode key) => Query(() => UnityEngine.Input.GetKey(key));

    public static bool CtrlHeld => Held(KeyCode.LeftControl) || Held(KeyCode.RightControl);
    public static bool AltHeld => Held(KeyCode.LeftAlt) || Held(KeyCode.RightAlt);
    public static bool ShiftHeld => Held(KeyCode.LeftShift) || Held(KeyCode.RightShift);
    public static bool AnyModifierHeld => CtrlHeld || AltHeld || ShiftHeld;

    private static bool Query(Func<bool> read)
    {
        if (_unavailable)
            return false;
        try
        {
            return read();
        }
        catch (InvalidOperationException exception)
        {
            // Thrown when the player is built with the Input System only.
            _unavailable = true;
            ModLog.Error($"Legacy keyboard input is unavailable in this game build: {exception.Message}");
            return false;
        }
    }
}
