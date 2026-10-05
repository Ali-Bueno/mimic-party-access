using System.Text.RegularExpressions;

namespace AccessKit.Text;

/// <summary>Turns on-screen UI text into speakable text (rich-text tags, invisible characters, whitespace).</summary>
public static class TextCleaner
{
    private static readonly Regex RichTextTag = new("<[^<>]{1,64}>", RegexOptions.Compiled);
    private static readonly Regex Whitespace = new(@"\s+", RegexOptions.Compiled);
    // Chevrons and arrows standing alone are drawer/carousel decorations; screen readers would say "greater than".
    private static readonly Regex DecorativeArrows = new(@"(?<=^|\s)[<>«»‹›←→↑↓▶◀►◄▲▼]+(?=\s|$)", RegexOptions.Compiled);
    private static readonly Regex TrailingNumber = new(@"(\d+(?:[.,]\d+)?\s*%?)\s*$", RegexOptions.Compiled);
    private static readonly Regex CamelBoundary = new("(?<=[a-z0-9])(?=[A-Z])", RegexOptions.Compiled);

    /// <summary>The number (with its % sign) a label ends with ("Music 80 %" → "80 %"), or null.</summary>
    public static string? TrailingQuantity(string text)
    {
        var match = TrailingNumber.Match(text);
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>Last-resort readable name from an identifier ("SettingsButton" → "Settings button").</summary>
    public static string Humanize(string identifier)
    {
        var words = CamelBoundary.Replace(identifier.Replace('_', ' '), " ").Trim();
        return words.Length == 0 ? identifier : char.ToUpperInvariant(words[0]) + words[1..].ToLowerInvariant();
    }

    public static string Clean(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        var withoutTags = RichTextTag.Replace(text, " ");
        var visible = withoutTags
            .Replace('\u00A0', ' ')
            .Replace("\u200B", "")
            .Replace("\u00AD", "");
        return Whitespace.Replace(DecorativeArrows.Replace(visible, " "), " ").Trim();
    }
}
