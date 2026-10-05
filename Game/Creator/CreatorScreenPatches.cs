using HarmonyLib;
using Mimick.Networking;
using Mimick.UI;
using MimicPartyAccess.Game.Announcements;
using UnityEngine.UI;

namespace MimicPartyAccess.Game.Creator;

[HarmonyPatch(typeof(CreatorUi), nameof(CreatorUi.NewIconButton))]
internal static class CreatorIconButtonPatch
{
    [HarmonyPostfix]
    private static void Postfix(string name, UiIcons.Kind kind, Button __result) =>
        IconNames.Remember(__result, name, kind);
}

[HarmonyPatch(typeof(ThemeCreatorScreen), nameof(ThemeCreatorScreen.Say))]
internal static class CreatorToastPatch
{
    [HarmonyPostfix]
    private static void Postfix(string message) =>
        AnnouncementGuard.Run("creator.toast", () => CreatorAnnouncer.OnToast(message));
}

[HarmonyPatch(typeof(ThemeCreatorScreen), nameof(ThemeCreatorScreen.StartRecording))]
internal static class CreatorStartRecordingPatch
{
    [HarmonyPostfix]
    private static void Postfix(ThemeCreatorScreen __instance) =>
        AnnouncementGuard.Run("creator.record", () => CreatorAnnouncer.OnRecordingStarted(__instance));
}

[HarmonyPatch(typeof(ThemeCreatorScreen), nameof(ThemeCreatorScreen.SetRecordLabel))]
internal static class CreatorRecordLabelPatch
{
    [HarmonyPostfix]
    private static void Postfix(ThemeCreatorScreen __instance, string label) =>
        AnnouncementGuard.Run("creator.record.label", () => CreatorAnnouncer.OnRecordLabel(__instance, label));
}

[HarmonyPatch(typeof(ThemeCreatorScreen), nameof(ThemeCreatorScreen.SetUploading))]
internal static class CreatorUploadingPatch
{
    [HarmonyPostfix]
    private static void Postfix(bool uploading) =>
        AnnouncementGuard.Run("creator.upload", () => CreatorAnnouncer.OnUploading(uploading));
}

[HarmonyPatch(typeof(ThemeCreatorScreen), nameof(ThemeCreatorScreen.OnPublished))]
internal static class CreatorPublishedPatch
{
    [HarmonyPostfix]
    private static void Postfix(WorkshopPublishResult result) =>
        AnnouncementGuard.Run("creator.published", () => CreatorAnnouncer.OnPublished(result));
}

[HarmonyPatch(typeof(ThemeCreatorScreen), nameof(ThemeCreatorScreen.SaveDraft))]
internal static class CreatorSavePatch
{
    [HarmonyPostfix]
    private static void Postfix(bool reveal, string __result) =>
        AnnouncementGuard.Run("creator.save", () => CreatorAnnouncer.OnSaved(reveal, __result));
}

[HarmonyPatch(typeof(SoundTrimPanel), nameof(SoundTrimPanel.Notice))]
internal static class CreatorTrimNoticePatch
{
    [HarmonyPostfix]
    private static void Postfix(string message) =>
        AnnouncementGuard.Run("creator.trim.notice", () => CreatorAnnouncer.OnTrimNotice(message));
}
