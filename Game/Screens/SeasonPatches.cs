using HarmonyLib;
using Mimick.Core;
using Mimick.UI;
using MimicPartyAccess.Game.Announcements;

namespace MimicPartyAccess.Game.Screens;

[HarmonyPatch(typeof(SeasonPassPanel), nameof(SeasonPassPanel.SetOpen))]
internal static class SeasonPanelOpenedPatch
{
    [HarmonyPostfix]
    private static void Postfix(SeasonPassPanel __instance) =>
        AnnouncementGuard.Run("season.open", () => SeasonAnnouncer.OnOpened(__instance));
}

[HarmonyPatch(typeof(SeasonPassPanel), nameof(SeasonPassPanel.ShowTier))]
internal static class SeasonShowTierPatch
{
    [HarmonyPostfix]
    private static void Postfix(SeasonPassPanel __instance) =>
        AnnouncementGuard.Run("season.tier", () => SeasonAnnouncer.OnPreviewChanged(__instance));
}

[HarmonyPatch(typeof(SeasonPassPanel), nameof(SeasonPassPanel.RefreshPreview))]
internal static class SeasonRefreshPreviewPatch
{
    [HarmonyPostfix]
    private static void Postfix(SeasonPassPanel __instance) =>
        AnnouncementGuard.Run("season.preview", () => SeasonAnnouncer.OnPreviewChanged(__instance));
}

// Callback of the claim request started by Select(tier, premium).
[HarmonyPatch(typeof(SeasonPassPanel), "_Select_b__59_0")]
internal static class SeasonClaimResultPatch
{
    [HarmonyPostfix]
    private static void Postfix(SeasonPassPanel __instance, ClaimResult result) =>
        AnnouncementGuard.Run("season.claim", () => SeasonAnnouncer.OnClaimResult(__instance, result));
}

[HarmonyPatch(typeof(SeasonPassPanel), nameof(SeasonPassPanel.ClaimAll))]
internal static class SeasonClaimAllPatch
{
    [HarmonyPostfix]
    private static void Postfix() => AnnouncementGuard.Run("season.claimall", SeasonAnnouncer.OnClaimAll);
}

[HarmonyPatch(typeof(SeasonPassPanel), nameof(SeasonPassPanel.BuyPass))]
internal static class SeasonBuyPassPatch
{
    [HarmonyPostfix]
    private static void Postfix() => AnnouncementGuard.Run("season.buy", SeasonAnnouncer.OnBuying);
}

[HarmonyPatch(typeof(SeasonPassPanel), "_BuyPass_b__74_0")]
internal static class SeasonBuyResultPatch
{
    [HarmonyPostfix]
    private static void Postfix(PurchaseResult result) =>
        AnnouncementGuard.Run("season.buy.result", () => ShopAnnouncer.OnPurchaseResult(result));
}

[HarmonyPatch(typeof(SeasonPassPanel), "_OfferPass_b__76_1")]
internal static class SeasonOfferResultPatch
{
    [HarmonyPostfix]
    private static void Postfix(PurchaseResult result) =>
        AnnouncementGuard.Run("season.offer.result", () => ShopAnnouncer.OnPurchaseResult(result));
}

[HarmonyPatch(typeof(SeasonPassPanel), nameof(SeasonPassPanel.OnPassPurchased))]
internal static class SeasonPassPurchasedPatch
{
    [HarmonyPostfix]
    private static void Postfix() => AnnouncementGuard.Run("season.purchased", SeasonAnnouncer.OnPassPurchased);
}

[HarmonyPatch(typeof(SeasonPassPanel), nameof(SeasonPassPanel.Celebrate))]
internal static class SeasonCelebratePatch
{
    [HarmonyPostfix]
    private static void Postfix(int bucks) =>
        AnnouncementGuard.Run("season.celebrate", () => SeasonAnnouncer.OnCelebrate(bucks));
}

[HarmonyPatch(typeof(SeasonXpBar), nameof(SeasonXpBar.PlayLevelUp))]
internal static class SeasonLevelUpPatch
{
    [HarmonyPostfix]
    private static void Postfix() => AnnouncementGuard.Run("season.levelup", SeasonAnnouncer.OnLevelUp);
}

[HarmonyPatch(typeof(SeasonXpBar), nameof(SeasonXpBar.Show))]
internal static class SeasonXpShowPatch
{
    [HarmonyPostfix]
    private static void Postfix(SeasonXpBar __instance) =>
        AnnouncementGuard.Run("season.xp", () => SeasonAnnouncer.OnXpShown(__instance));
}
