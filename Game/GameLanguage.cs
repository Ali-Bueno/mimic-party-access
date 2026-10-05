using UnityEngine.Localization.Settings;

namespace MimicPartyAccess.Game;

/// <summary>Reads the language the player selected in Mimic Party (Unity Localization's selected locale).</summary>
public static class GameLanguage
{
    public static string? Detect()
    {
        try
        {
            var locale = LocalizationSettings.SelectedLocale;
            return locale == null ? null : locale.Identifier.Code;
        }
        catch (Exception exception)
        {
            AccessKit.ModLog.WarningOnce("game-language", $"Could not read the game's selected locale: {exception.Message}");
            return null;
        }
    }
}
