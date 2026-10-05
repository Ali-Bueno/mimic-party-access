using BepInEx.Configuration;

namespace AccessKit;

/// <summary>User-facing toggles (BepInEx/config/&lt;guid&gt;.cfg); every feature can be switched off independently.</summary>
public static class ModConfig
{
    private static ConfigEntry<bool>? _menus;
    private static ConfigEntry<bool>? _gameAnnouncements;
    private static ConfigEntry<bool>? _logSpeech;
    private static ConfigEntry<bool>? _uiDump;
    private static ConfigEntry<bool>? _logUi;

    public static bool Menus => _menus?.Value ?? true;
    public static bool GameAnnouncements => _gameAnnouncements?.Value ?? true;
    public static bool LogSpeech => _logSpeech?.Value ?? false;
    public static bool UiDump => _uiDump?.Value ?? false;
    public static bool LogUi => _logUi?.Value ?? false;

    public static void Bind(ConfigFile config)
    {
        _menus = config.Bind("Features", "Menus", true, "Keyboard navigation and screen-reader output for every menu.");
        _gameAnnouncements = config.Bind("Features", "GameAnnouncements", true, "Announce match events (phases, turns, scores, results).");
        _logSpeech = config.Bind("Debug", "LogSpeech", true, "Write every spoken line to the BepInEx log.");
        _logUi = config.Bind("Debug", "LogUi", true, "Log menu layer changes, focus moves and activations.");
        _uiDump = config.Bind("Debug", "UiDump", true, "Write a UI structure dump (ui-dump.txt) whenever the active menu layer changes, and on Ctrl+Shift+F12.");
    }
}
