using AccessKit.Speech;
using AccessKit.Text;
using HarmonyLib;
using Mimick.UI;

namespace MimicPartyAccess.Game.Announcements;

/// <summary>The player introduction banner at the start of a match: a name and a win counter that counts up with a sound.</summary>
[HarmonyPatch(typeof(PlayerIntroBanner), nameof(PlayerIntroBanner.Show))]
internal static class PlayerIntroBannerShowPatch
{
    [HarmonyPostfix]
    private static void Postfix(string playerName, int wins) =>
        AnnouncementGuard.Run("intro", () => ScreenReader.Say(Strings.Get("game.intro.player", TextCleaner.Clean(playerName), wins)));
}
