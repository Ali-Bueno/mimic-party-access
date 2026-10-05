using AccessKit.Speech;
using AccessKit.Text;
using Mimick.Data;
using Mimick.UI;

namespace MimicPartyAccess.Game.Announcements;

/// <summary>Announces round start, phase changes, recording turn, countdown and scores with diff-gating.</summary>
internal static class RoundAnnouncer
{
    private const string PhaseKeyPrefix = "game.phase.";

    private static RoundPhase? _lastPhase;
    private static readonly List<(string Name, int Points)> RoundScores = new();

    public static void OnRoundStarted(GameScreen screen)
    {
        _lastPhase = null;
        RoundScores.Clear();

        var gameLabel = TextCleaner.Clean(screen.roundLabel?.text);
        var text = string.IsNullOrWhiteSpace(gameLabel) ? Strings.Get("game.round.started") : gameLabel;
        ScreenReader.Say(text, interrupt: true);
    }

    public static void OnPhaseChanged(GameScreen screen, RoundPhase phase)
    {
        if (_lastPhase == phase)
            return;
        _lastPhase = phase;

        if (!Strings.TryGet(PhaseKeyPrefix + phase.ToString().ToLowerInvariant(), out var text))
            return;

        if (phase == RoundPhase.RoundResults)
        {
            var detail = TakeScoreSummary() ?? RoundStandings.Describe(screen.roundController);
            if (!string.IsNullOrWhiteSpace(detail))
                text = $"{text} {detail}";
        }

        ScreenReader.Say(text, interrupt: true);
    }

    public static void OnScored(MimickPlayer player, float score)
    {
        var name = TextCleaner.Clean(player.DisplayName);
        if (string.IsNullOrWhiteSpace(name))
        {
            ScreenReader.Say(Strings.Get("game.score.unavailable"));
            return;
        }

        var points = (int)Math.Round(score);
        RoundScores.Add((name, points));
        ScreenReader.Say(Strings.Get("game.score.result", name, points));
    }

    public static void OnRecordTurnStarted(MimickPlayer player)
    {
        if (player.IsLocal)
            ScreenReader.Say(Strings.Get("game.record.your_turn"), interrupt: true);
    }

    public static void OnCountdownTick(GameScreen screen, int value)
    {
        if (screen.roundController?.IsRecording == true)
            return;

        ScreenReader.Say(Strings.Get("game.record.countdown", value));
    }

    private static string? TakeScoreSummary()
    {
        if (RoundScores.Count == 0)
            return null;

        var list = string.Join(", ", RoundScores.Select(entry => $"{entry.Name} {entry.Points}"));
        RoundScores.Clear();
        return Strings.Get("game.round.scores", list);
    }
}
