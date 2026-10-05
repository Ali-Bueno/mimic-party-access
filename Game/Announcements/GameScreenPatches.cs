using HarmonyLib;
using Mimick.Data;
using Mimick.Scoring;
using Mimick.UI;

namespace MimicPartyAccess.Game.Announcements;

[HarmonyPatch(typeof(GameScreen), nameof(GameScreen.OnRoundStarted))]
internal static class GameScreenRoundStartedPatch
{
    [HarmonyPostfix]
    private static void Postfix(GameScreen __instance) =>
        AnnouncementGuard.Run("round", () => RoundAnnouncer.OnRoundStarted(__instance));
}

[HarmonyPatch(typeof(GameScreen), nameof(GameScreen.OnPhaseChanged))]
internal static class GameScreenPhaseChangedPatch
{
    [HarmonyPostfix]
    private static void Postfix(GameScreen __instance, RoundPhase phase) =>
        AnnouncementGuard.Run("phase", () => RoundAnnouncer.OnPhaseChanged(__instance, phase));
}

[HarmonyPatch(typeof(GameScreen), nameof(GameScreen.OnScored))]
internal static class GameScreenScoredPatch
{
    [HarmonyPostfix]
    private static void Postfix(MimickPlayer player, ScoreResult score) =>
        AnnouncementGuard.Run("score", () => RoundAnnouncer.OnScored(player, score.Score));
}

[HarmonyPatch(typeof(GameScreen), nameof(GameScreen.OnRecordTurnStarted))]
internal static class GameScreenRecordTurnPatch
{
    [HarmonyPostfix]
    private static void Postfix(MimickPlayer player) =>
        AnnouncementGuard.Run("turn", () => RoundAnnouncer.OnRecordTurnStarted(player));
}

[HarmonyPatch(typeof(GameScreen), nameof(GameScreen.OnCountdownTick))]
internal static class GameScreenCountdownPatch
{
    [HarmonyPostfix]
    private static void Postfix(GameScreen __instance, int value) =>
        AnnouncementGuard.Run("countdown", () => RoundAnnouncer.OnCountdownTick(__instance, value));
}
