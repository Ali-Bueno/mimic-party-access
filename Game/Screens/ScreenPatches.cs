namespace MimicPartyAccess.Game.Screens;

/// <summary>Every Harmony patch class of the season pass, objectives and shop announcements.</summary>
public static class ScreenPatches
{
    public static readonly Type[] PatchClasses =
    {
        typeof(SeasonPanelOpenedPatch), typeof(SeasonShowTierPatch), typeof(SeasonRefreshPreviewPatch),
        typeof(SeasonClaimResultPatch), typeof(SeasonClaimAllPatch), typeof(SeasonBuyPassPatch),
        typeof(SeasonBuyResultPatch), typeof(SeasonOfferResultPatch), typeof(SeasonPassPurchasedPatch),
        typeof(SeasonCelebratePatch), typeof(SeasonLevelUpPatch), typeof(SeasonXpShowPatch),
        typeof(ObjectivesOpenedPatch), typeof(ObjectivesRefreshPatch),
        typeof(ShopOpenedPatch), typeof(ShopResultPatch), typeof(ShopDeferredPatch), typeof(ShopBalancePatch),
        typeof(ShopStatusPatch), typeof(PriceTagApplyPatch), typeof(RestoreResultPatch),
    };
}
