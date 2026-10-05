using AccessKit.Speech;
using AccessKit.Text;
using Mimick.UI;

namespace MimicPartyAccess.Game.Announcements;

/// <summary>Announces the lobby roster when it changes.</summary>
internal static class LobbyAnnouncer
{
    private static string? _lastSummary;

    public static void Reset() => _lastSummary = null;

    public static void OnRosterRefreshed(LobbyScreen screen)
    {
        var rows = ReadRows(screen);
        var summary = rows.Count > 0 ? string.Join(". ", rows) : ReadFallback(screen);
        if (string.IsNullOrWhiteSpace(summary) || summary == _lastSummary)
            return;

        _lastSummary = summary;
        ScreenReader.Say(Strings.Get("game.lobby.players", summary));
    }

    private static List<string> ReadRows(LobbyScreen screen)
    {
        var rows = new List<string>();
        var root = screen.rosterList;
        if (root == null)
            return rows;

        for (var i = 0; i < root.childCount; i++)
        {
            var row = root.GetChild(i)?.GetComponentInChildren<PlayerLobbyRowUI>();
            var label = TextCleaner.Clean(row?.label?.text);
            if (string.IsNullOrWhiteSpace(label))
                continue;

            var status = TextCleaner.Clean(row?.status?.text);
            rows.Add(string.IsNullOrWhiteSpace(status) ? label : $"{label}, {status}");
        }

        return rows;
    }

    private static string ReadFallback(LobbyScreen screen)
    {
        var parts = new[] { screen.playerListLabel?.text, screen.themeLabel?.text, screen.hintLabel?.text }
            .Select(text => TextCleaner.Clean(text))
            .Where(text => !string.IsNullOrWhiteSpace(text));
        return string.Join(". ", parts);
    }
}
