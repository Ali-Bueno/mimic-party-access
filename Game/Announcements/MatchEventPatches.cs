using HarmonyLib;
using Mimick.Data;
using Mimick.Networking;
using Mimick.UI;

namespace MimicPartyAccess.Game.Announcements;

[HarmonyPatch(typeof(GameScreen), nameof(GameScreen.OnPlaybackStarted))]
internal static class GameScreenPlaybackStartedPatch
{
    [HarmonyPostfix]
    private static void Postfix(MimickPlayer player) =>
        AnnouncementGuard.Run("playback", () => MatchEventAnnouncer.OnPlaybackStarted(player));
}

[HarmonyPatch(typeof(GameScreen), nameof(GameScreen.OnMicrophoneFailed))]
internal static class GameScreenMicrophoneFailedPatch
{
    [HarmonyPostfix]
    private static void Postfix(string reason) =>
        AnnouncementGuard.Run("microphone", () => MatchEventAnnouncer.OnMicrophoneFailed(reason));
}

[HarmonyPatch(typeof(GameScreen), nameof(GameScreen.OnNetworkStatusChanged))]
internal static class GameScreenNetworkStatusPatch
{
    [HarmonyPostfix]
    private static void Postfix(NetworkStatus status) =>
        AnnouncementGuard.Run("network", () => MatchEventAnnouncer.OnNetworkStatusChanged(status));
}

[HarmonyPatch(typeof(GameScreen), nameof(GameScreen.OnHostLost))]
internal static class GameScreenHostLostPatch
{
    [HarmonyPostfix]
    private static void Postfix() =>
        AnnouncementGuard.Run("host.lost", MatchEventAnnouncer.OnHostLost);
}

[HarmonyPatch(typeof(GameScreen), nameof(GameScreen.OnTeamTurnStarted))]
internal static class GameScreenTeamTurnPatch
{
    [HarmonyPostfix]
    private static void Postfix(TeamSide side, int members) =>
        AnnouncementGuard.Run("team.turn", () => MatchEventAnnouncer.OnTeamTurnStarted(side, members));
}

[HarmonyPatch(typeof(GameScreen), nameof(GameScreen.OnGameFinished))]
internal static class GameScreenGameFinishedPatch
{
    [HarmonyPostfix]
    private static void Postfix() =>
        AnnouncementGuard.Run("game.finished", MatchEventAnnouncer.OnGameFinished);
}

// Play(slots, seconds) only builds the lineup coroutine; the slots are the sabotages shown at round start.
[HarmonyPatch(typeof(MalusLineup), nameof(MalusLineup.Play))]
internal static class MalusLineupPlayPatch
{
    [HarmonyPostfix]
    private static void Postfix(Il2CppSystem.Collections.Generic.IReadOnlyList<WheelSlot> __0) =>
        AnnouncementGuard.Run("malus.lineup", () => MatchEventAnnouncer.OnMalusLineup(__0));
}
