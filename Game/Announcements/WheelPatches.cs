using HarmonyLib;
using Mimick.Data;
using Mimick.UI;

namespace MimicPartyAccess.Game.Announcements;

[HarmonyPatch(typeof(GameScreen), nameof(GameScreen.OnWheelStopped))]
internal static class GameScreenWheelStoppedPatch
{
    [HarmonyPostfix]
    private static void Postfix(WheelSlot slot) =>
        AnnouncementGuard.Run("wheel.stopped", () => WheelAnnouncer.OnStopped(slot));
}

// The candidate list is the method's only parameter (named "_" in the game), bound by position.
[HarmonyPatch(typeof(GameScreen), nameof(GameScreen.OnWheelTargetingStarted))]
internal static class GameScreenWheelTargetingPatch
{
    [HarmonyPostfix]
    private static void Postfix(GameScreen __instance, Il2CppSystem.Collections.Generic.IReadOnlyList<MimickPlayer> __0) =>
        AnnouncementGuard.Run("wheel.targeting", () => WheelAnnouncer.OnTargetingStarted(__instance, __0));
}

[HarmonyPatch(typeof(GameScreen), nameof(GameScreen.OnWheelTargetingEnded))]
internal static class GameScreenWheelTargetingEndedPatch
{
    [HarmonyPostfix]
    private static void Postfix() =>
        AnnouncementGuard.Run("wheel.targeting.end", WheelAnnouncer.OnTargetingEnded);
}

[HarmonyPatch(typeof(GameScreen), nameof(GameScreen.OnWheelEvent))]
internal static class GameScreenWheelEventPatch
{
    [HarmonyPostfix]
    private static void Postfix(string line) =>
        AnnouncementGuard.Run("wheel.event", () => WheelAnnouncer.OnEvent(line));
}
