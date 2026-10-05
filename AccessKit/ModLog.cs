using BepInEx.Logging;

namespace AccessKit;

/// <summary>Static access to the plugin logger so services and patches can log without an instance.</summary>
public static class ModLog
{
    private static ManualLogSource? _source;
    private static readonly HashSet<string> OnceKeys = new();

    public static void Initialize(ManualLogSource source) => _source = source;

    public static void Info(string message) => _source?.LogInfo(message);
    public static void Warning(string message) => _source?.LogWarning(message);
    public static void Error(string message) => _source?.LogError(message);
    public static void Debug(string message) => _source?.LogDebug(message);

    /// <summary>Logs a warning only the first time a given key is seen (for per-frame failure paths).</summary>
    public static void WarningOnce(string key, string message)
    {
        if (OnceKeys.Add(key))
            Warning(message);
    }
}
