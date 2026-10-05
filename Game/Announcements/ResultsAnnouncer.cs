using AccessKit.Speech;
using AccessKit.Text;
using Mimick.UI;

namespace MimicPartyAccess.Game.Announcements;

/// <summary>Announces the final ranking once per results screen.</summary>
internal static class ResultsAnnouncer
{
    private static string? _lastSummary;

    public static void Reset() => _lastSummary = null;

    public static void OnReplayWindowOpened(ResultsScreen screen)
    {
        var entries = ReadCards(screen);
        var summary = entries.Count > 0 ? string.Join(" ", entries) : ReadFallback(screen);
        if (string.IsNullOrWhiteSpace(summary) || summary == _lastSummary)
            return;

        _lastSummary = summary;
        ScreenReader.Say(Strings.Get("game.results.summary", summary), interrupt: true);
    }

    private static List<string> ReadCards(ResultsScreen screen)
    {
        var entries = new List<string>();
        var cards = screen.cards;
        if (cards == null)
            return entries;

        for (var i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            var name = TextCleaner.Clean(card?.nameLabel?.text);
            if (card == null || string.IsNullOrWhiteSpace(name))
                continue;

            var rank = TextCleaner.Clean(card.rankLabel?.text);
            var score = TextCleaner.Clean(card.scoreLabel?.text);
            entries.Add(Strings.Get("game.results.player",
                string.IsNullOrWhiteSpace(rank) ? (entries.Count + 1).ToString() : rank,
                name,
                string.IsNullOrWhiteSpace(score) ? Strings.Get("game.score.unavailable") : score));
        }

        return entries;
    }

    private static string ReadFallback(ResultsScreen screen)
    {
        var parts = new[] { screen.podiumLabel?.text, screen.statsLabel?.text }
            .Select(text => TextCleaner.Clean(text))
            .Where(text => !string.IsNullOrWhiteSpace(text));
        return string.Join(". ", parts);
    }
}
