using System.Globalization;

namespace AccessKit.Text;

/// <summary>
/// The single layer for mod-authored text. Strings live in <c>localization/&lt;lang&gt;/*.txt</c> as
/// <c>key=value</c> lines; every file of a language is merged, English fills missing keys.
/// </summary>
public static class Strings
{
    public const string FallbackLanguage = "en";

    private static string _root = "";
    private static Dictionary<string, string> _fallback = new();
    private static Dictionary<string, string> _current = new();
    private static readonly HashSet<string> MissingKeys = new();

    public static string Language { get; private set; } = FallbackLanguage;

    /// <summary>Increments whenever the active language changes, so caches of spoken text can be invalidated.</summary>
    public static int Revision { get; private set; }

    public static void Initialize(string localizationDirectory)
    {
        _root = localizationDirectory;
        _fallback = LoadLanguage(FallbackLanguage);
        SetLanguage(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
    }

    /// <summary>Switches to the given ISO language code; unknown languages fall back to English.</summary>
    public static void SetLanguage(string? languageCode)
    {
        var code = Normalize(languageCode);
        if (code == Language && Revision > 0)
            return;

        Language = Directory.Exists(Path.Combine(_root, code)) ? code : FallbackLanguage;
        _current = Language == FallbackLanguage ? _fallback : LoadLanguage(Language);
        Revision++;
        ModLog.Info($"Mod language: {Language} (requested '{languageCode}').");
    }

    public static string Get(string key, params object[] args)
    {
        if (!_current.TryGetValue(key, out var value) && !_fallback.TryGetValue(key, out value))
        {
            if (MissingKeys.Add(key))
                ModLog.Warning($"Missing localization key '{key}'.");
            return key;
        }

        return args.Length == 0 ? value : string.Format(CultureInfo.InvariantCulture, value, args);
    }

    public static bool TryGet(string key, out string value)
    {
        return _current.TryGetValue(key, out value!) || _fallback.TryGetValue(key, out value!);
    }

    private static Dictionary<string, string> LoadLanguage(string code)
    {
        var table = new Dictionary<string, string>(StringComparer.Ordinal);
        var directory = Path.Combine(_root, code);
        if (!Directory.Exists(directory))
            return table;

        foreach (var file in Directory.GetFiles(directory, "*.txt").OrderBy(path => path, StringComparer.Ordinal))
            foreach (var (key, value) in ParseLines(File.ReadAllLines(file)))
                table[key] = value;
        return table;
    }

    public static IEnumerable<(string Key, string Value)> ParseLines(IEnumerable<string> lines)
    {
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            var separator = line.IndexOf('=');
            if (separator <= 0)
                continue;

            yield return (line[..separator].Trim(), line[(separator + 1)..].Trim().Replace("\\n", "\n"));
        }
    }

    private static string Normalize(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            return FallbackLanguage;
        var code = languageCode.Trim().ToLowerInvariant();
        var separator = code.IndexOfAny(new[] { '-', '_' });
        return separator > 0 ? code[..separator] : code;
    }
}
