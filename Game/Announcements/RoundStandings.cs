using AccessKit.Text;
using Il2CppInterop.Runtime;
using Mimick.Data;
using Mimick.Gameplay;

namespace MimicPartyAccess.Game.Announcements;

/// <summary>Describes the leader and the local player's score from the round roster.</summary>
internal static class RoundStandings
{
    /// <summary>Every player's total, highest first, with the local player marked (the S key during a match).</summary>
    public static string? DescribeAll(RoundController? controller)
    {
        var roster = controller?.Roster;
        var collection = roster?.TryCast<Il2CppSystem.Collections.Generic.ICollection<MimickPlayer>>();
        if (roster == null || collection == null)
            return null;

        var entries = new List<(string Name, int Score)>();
        for (var i = 0; i < collection.Count; i++)
        {
            var player = roster[i];
            var name = TextCleaner.Clean(player?.DisplayName);
            if (player == null || string.IsNullOrWhiteSpace(name))
                continue;
            entries.Add((player.IsLocal ? Strings.Get("game.standings.you", name) : name, player.Score));
        }

        if (entries.Count == 0)
            return null;
        var list = string.Join(", ", entries.OrderByDescending(entry => entry.Score).Select(entry => Strings.Get("game.standings.entry", entry.Name, entry.Score)));
        return Strings.Get("game.standings", list);
    }

    public static string? Describe(RoundController? controller)
    {
        var roster = controller?.Roster;
        var collection = roster?.TryCast<Il2CppSystem.Collections.Generic.ICollection<MimickPlayer>>();
        if (roster == null || collection == null)
            return null;

        string? leader = null;
        var leaderScore = int.MinValue;
        string? localName = null;
        var localScore = 0;

        for (var i = 0; i < collection.Count; i++)
        {
            var player = roster[i];
            var name = TextCleaner.Clean(player?.DisplayName);
            if (player == null || string.IsNullOrWhiteSpace(name))
                continue;

            if (player.Score > leaderScore)
            {
                leaderScore = player.Score;
                leader = name;
            }

            if (player.IsLocal)
            {
                localName = name;
                localScore = player.Score;
            }
        }

        if (leader == null)
            return null;

        var text = Strings.Get("game.round.leader", leader, leaderScore);
        if (localName != null && !string.Equals(localName, leader, StringComparison.Ordinal))
            text = $"{text} {Strings.Get("game.round.your_score", localScore)}";
        return text;
    }
}
