using HarmonyLib;
using Mimick.Audio;
using Mimick.UI;

namespace MimicPartyAccess.Game.Announcements;

[HarmonyPatch(typeof(GameScreen), nameof(GameScreen.OnPerformanceReady))]
internal static class GameScreenPerformanceReadyPatch
{
    [HarmonyPostfix]
    private static void Postfix(AudioSamples reference, float leadIn, float tail) =>
        AnnouncementGuard.Run("guide.ready", () => RecordingGuide.OnPerformanceReady(reference, leadIn, tail));
}

// The bar also shows other players' takes; the guide only plays while the local player records.
[HarmonyPatch(typeof(GameScreen), nameof(GameScreen.OnPerformanceProgress))]
internal static class GameScreenPerformanceProgressPatch
{
    [HarmonyPostfix]
    private static void Postfix(GameScreen __instance, float progress)
    {
        if (__instance.roundController?.IsRecording == true)
            AnnouncementGuard.Run("guide.progress", () => RecordingGuide.OnProgress(progress));
    }
}
