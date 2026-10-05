using AccessKit.Speech;
using AccessKit.Text;
using AccessKit.Ui;
using Mimick.UI;
using UnityEngine;

namespace MimicPartyAccess.Game.Announcements;

/// <summary>
/// Pack downloads from the workshop: started, progress in quarters, finished. Progress is the game's own
/// <c>LobbyThemePanel.Progress(itemId)</c> text, polled while downloads are pending.
/// </summary>
internal static class DownloadAnnouncer
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(1);

    // Progress is announced at each quarter so long downloads report without speaking every percent.
    private const int ProgressStep = 25;

    private static readonly Dictionary<string, (string Title, int Announced)> Pending = new();
    private static DateTime _nextPoll;

    public static void OnStarted(string itemId)
    {
        var title = PackTitle(itemId);
        Pending[itemId] = (title, 0);
        ScreenReader.Say(Strings.Get("game.download.started", title));
    }

    public static void OnStartedAll() =>
        ScreenReader.Say(Strings.Get("game.download.all_started"));

    // The installed set changed: every pending pack the game no longer reports progress for has finished.
    public static void OnInstalledChanged()
    {
        foreach (var itemId in Pending.Keys.ToList())
        {
            if (Percent(itemId) is int percent && percent < 100)
                continue;
            ScreenReader.Say(Strings.Get("game.download.finished", Pending[itemId].Title));
            Pending.Remove(itemId);
        }
    }

    public static void Tick()
    {
        if (Pending.Count == 0 || DateTime.UtcNow < _nextPoll)
            return;
        _nextPoll = DateTime.UtcNow + PollInterval;

        foreach (var itemId in Pending.Keys.ToList())
        {
            if (Percent(itemId) is not int percent)
                continue;
            var (title, announced) = Pending[itemId];
            var step = percent / ProgressStep * ProgressStep;
            if (step <= announced || step >= 100)
                continue;
            Pending[itemId] = (title, step);
            ScreenReader.Say(Strings.Get("game.download.progress", title, step));
        }
    }

    private static int? Percent(string itemId)
    {
        var text = TextCleaner.Clean(LobbyThemePanel.Progress(itemId));
        var digits = string.IsNullOrEmpty(text) ? null : TextCleaner.TrailingQuantity(text.Replace("%", "").Trim());
        return int.TryParse(digits?.Trim(), out var percent) ? percent : null;
    }

    // Workshop cards are named Item_<id> and lobby pack rows Theme_<id>; their largest text is the pack name.
    private static string PackTitle(string itemId)
    {
        foreach (var name in new[] { "Item_" + itemId, "Theme_" + itemId })
        {
            var row = GameObject.Find(name);
            if (row == null)
                continue;
            var texts = UiQueries.GetVisibleTexts(row.transform);
            if (texts.Count > 0)
                return UiQueries.ReadText(texts.MaxBy(text => text.fontSize));
        }
        return Strings.Get("game.download.unnamed");
    }
}
