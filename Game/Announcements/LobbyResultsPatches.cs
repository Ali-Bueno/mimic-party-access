using HarmonyLib;
using Mimick.UI;

namespace MimicPartyAccess.Game.Announcements;

[HarmonyPatch(typeof(LobbyScreen), nameof(LobbyScreen.Start))]
internal static class LobbyStartPatch
{
    [HarmonyPrefix]
    private static void Prefix() => LobbyAnnouncer.Reset();
}

[HarmonyPatch(typeof(LobbyScreen), nameof(LobbyScreen.RefreshRoster))]
internal static class LobbyRosterPatch
{
    [HarmonyPostfix]
    private static void Postfix(LobbyScreen __instance) =>
        AnnouncementGuard.Run("lobby", () => LobbyAnnouncer.OnRosterRefreshed(__instance));
}

[HarmonyPatch(typeof(ResultsScreen), nameof(ResultsScreen.Start))]
internal static class ResultsStartPatch
{
    [HarmonyPrefix]
    private static void Prefix() => ResultsAnnouncer.Reset();
}

[HarmonyPatch(typeof(ResultsScreen), nameof(ResultsScreen.OpenReplayWindow))]
internal static class ResultsReplayWindowPatch
{
    [HarmonyPostfix]
    private static void Postfix(ResultsScreen __instance) =>
        AnnouncementGuard.Run("results", () => ResultsAnnouncer.OnReplayWindowOpened(__instance));
}
