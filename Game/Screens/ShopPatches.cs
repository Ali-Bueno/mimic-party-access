using HarmonyLib;
using Mimick.Core;
using Mimick.UI;
using MimicPartyAccess.Game.Announcements;
using UnityEngine.UI;

namespace MimicPartyAccess.Game.Screens;

[HarmonyPatch(typeof(ObjectivesPanel), nameof(ObjectivesPanel.SetOpen))]
internal static class ObjectivesOpenedPatch
{
    [HarmonyPostfix]
    private static void Postfix(ObjectivesPanel __instance) =>
        AnnouncementGuard.Run("objectives.open", () => ShopAnnouncer.OnObjectivesOpened(__instance));
}

[HarmonyPatch(typeof(ObjectivesPanel), nameof(ObjectivesPanel.Refresh))]
internal static class ObjectivesRefreshPatch
{
    [HarmonyPostfix]
    private static void Postfix(ObjectivesPanel __instance) =>
        AnnouncementGuard.Run("objectives.refresh", () => ShopAnnouncer.OnObjectivesChanged(__instance));
}

[HarmonyPatch(typeof(MimiBucksShopPanel), nameof(MimiBucksShopPanel.SetOpen))]
internal static class ShopOpenedPatch
{
    [HarmonyPostfix]
    private static void Postfix(MimiBucksShopPanel __instance) =>
        AnnouncementGuard.Run("shop.open", () => ShopAnnouncer.OnShopOpened(__instance));
}

[HarmonyPatch(typeof(MimiBucksShopPanel), nameof(MimiBucksShopPanel.ReportResult))]
internal static class ShopResultPatch
{
    [HarmonyPostfix]
    private static void Postfix(PurchaseResult result) =>
        AnnouncementGuard.Run("shop.result", () => ShopAnnouncer.OnPurchaseResult(result));
}

[HarmonyPatch(typeof(MimiBucksShopPanel), nameof(MimiBucksShopPanel.OnPurchaseDeferred))]
internal static class ShopDeferredPatch
{
    [HarmonyPostfix]
    private static void Postfix() => AnnouncementGuard.Run("shop.deferred", ShopAnnouncer.OnPurchaseDeferred);
}

[HarmonyPatch(typeof(MimiBucksShopPanel), nameof(MimiBucksShopPanel.RefreshBalance))]
internal static class ShopBalancePatch
{
    [HarmonyPostfix]
    private static void Postfix(MimiBucksShopPanel __instance) =>
        AnnouncementGuard.Run("shop.balance", () => ShopAnnouncer.OnBalance(__instance));
}

[HarmonyPatch(typeof(MimiBucksShopPanel), nameof(MimiBucksShopPanel.RefreshStatus))]
internal static class ShopStatusPatch
{
    [HarmonyPostfix]
    private static void Postfix(MimiBucksShopPanel __instance) =>
        AnnouncementGuard.Run("shop.status", () => ShopAnnouncer.OnStatus(__instance));
}

[HarmonyPatch(typeof(CosmeticPriceTag), nameof(CosmeticPriceTag.Apply))]
internal static class PriceTagApplyPatch
{
    [HarmonyPostfix]
    private static void Postfix(Button button, int price) =>
        AnnouncementGuard.Run("shop.pricetag", () => PriceTags.Record(button, price));
}

[HarmonyPatch(typeof(RestorePurchasesFlow), nameof(RestorePurchasesFlow.Tell))]
internal static class RestoreResultPatch
{
    [HarmonyPostfix]
    private static void Postfix(bool ok) =>
        AnnouncementGuard.Run("shop.restore", () => ShopAnnouncer.OnRestored(ok));
}
