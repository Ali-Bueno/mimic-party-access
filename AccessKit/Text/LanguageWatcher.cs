namespace AccessKit.Text;

/// <summary>Keeps the mod language in step with the game's language setting, which the player can change at runtime.</summary>
public static class LanguageWatcher
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);
    private static Func<string?>? _detect;
    private static DateTime _nextPoll;

    public static void Initialize(Func<string?> detectGameLanguage) => _detect = detectGameLanguage;

    public static void Tick()
    {
        if (_detect == null || DateTime.UtcNow < _nextPoll)
            return;

        _nextPoll = DateTime.UtcNow + PollInterval;
        var code = _detect();
        if (!string.IsNullOrWhiteSpace(code))
            Strings.SetLanguage(code);
    }
}
