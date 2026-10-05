using AccessKit.Speech;
using AccessKit.Text;
using Mimick.Data;
using Mimick.Networking;

namespace MimicPartyAccess.Game.Announcements;

/// <summary>Announces playback turns, microphone and connection problems, team turns, sabotages and match end.</summary>
internal static class MatchEventAnnouncer
{
    private const string NetworkKeyPrefix = "game.network.";
    private const string TeamKeyPrefix = "game.team.";

    private static NetworkStatus? _lastStatus;
    private static bool _connectionProblem;
    private static (TeamSide Side, int Members)? _lastTeamTurn;

    public static void OnPlaybackStarted(MimickPlayer player)
    {
        if (player.IsLocal)
        {
            ScreenReader.Say(Strings.Get("game.playback.yours"));
            return;
        }

        var name = TextCleaner.Clean(player.DisplayName);
        if (name.Length > 0)
            ScreenReader.Say(Strings.Get("game.playback.player", name));
    }

    public static void OnMicrophoneFailed(string reason)
    {
        var detail = TextCleaner.Clean(reason);
        var text = Strings.Get("game.microphone.failed");
        ScreenReader.Say(detail.Length > 0 ? $"{text} {detail}" : text, interrupt: true);
    }

    public static void OnNetworkStatusChanged(NetworkStatus status)
    {
        if (_lastStatus == status)
            return;
        _lastStatus = status;

        var key = status switch
        {
            NetworkStatus.Disconnected => "disconnected",
            NetworkStatus.Failed => "failed",
            NetworkStatus.Searching => "reconnecting",
            NetworkStatus.InGame when _connectionProblem => "restored",
            _ => null,
        };

        _connectionProblem = status is NetworkStatus.Disconnected or NetworkStatus.Failed or NetworkStatus.Searching;
        if (key == null)
            return;

        ScreenReader.Say(Strings.Get(NetworkKeyPrefix + key), interrupt: status != NetworkStatus.InGame);
    }

    public static void OnHostLost() => ScreenReader.Say(Strings.Get("game.network.host_lost"), interrupt: true);

    public static void OnTeamTurnStarted(TeamSide side, int members)
    {
        if (_lastTeamTurn == (side, members))
            return;
        _lastTeamTurn = (side, members);

        if (!Strings.TryGet(TeamKeyPrefix + side.ToString().ToLowerInvariant(), out var team))
            return;

        ScreenReader.Say(Strings.Get("game.team.turn", team, members));
    }

    public static void OnGameFinished()
    {
        _lastTeamTurn = null;
        ScreenReader.Say(Strings.Get("game.match.finished"));
    }

    public static void OnMalusLineup(Il2CppSystem.Collections.Generic.IReadOnlyList<WheelSlot>? slots)
    {
        var count = slots?.TryCast<Il2CppSystem.Collections.Generic.ICollection<WheelSlot>>()?.Count ?? 0;
        var titles = new List<string>();
        for (var index = 0; index < count; index++)
        {
            var title = TextCleaner.Clean(slots![index]?.LocalizedTitle);
            if (title.Length > 0)
                titles.Add(title.TrimEnd('.'));
        }

        if (titles.Count > 0)
            ScreenReader.Say(Strings.Get("game.malus.lineup", string.Join(", ", titles)));
    }
}
