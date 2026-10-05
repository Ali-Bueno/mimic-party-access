using HarmonyLib;
using Mimick.UI;

namespace MimicPartyAccess.Game.Announcements;

[HarmonyPatch(typeof(LobbyThemePanel), nameof(LobbyThemePanel.StartDownload))]
internal static class LobbyThemeStartDownloadPatch
{
    [HarmonyPostfix]
    private static void Postfix(string itemId) =>
        AnnouncementGuard.Run("download.start", () => DownloadAnnouncer.OnStarted(itemId));
}

[HarmonyPatch(typeof(LobbyThemePanel), nameof(LobbyThemePanel.StartDownloadAll))]
internal static class LobbyThemeStartDownloadAllPatch
{
    [HarmonyPostfix]
    private static void Postfix() =>
        AnnouncementGuard.Run("download.all", DownloadAnnouncer.OnStartedAll);
}

[HarmonyPatch(typeof(LobbyThemePanel), nameof(LobbyThemePanel.OnInstalledChanged))]
internal static class LobbyThemeInstalledChangedPatch
{
    [HarmonyPostfix]
    private static void Postfix() =>
        AnnouncementGuard.Run("download.installed", DownloadAnnouncer.OnInstalledChanged);
}
