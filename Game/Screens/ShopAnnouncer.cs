using AccessKit.Speech;
using AccessKit.Text;
using AccessKit.Ui;
using Mimick.Core;
using Mimick.UI;
using TMPro;
using UnityEngine;

namespace MimicPartyAccess.Game.Screens;

/// <summary>MimiBucks shop and objectives: balance, status line, purchase and restore results, objective progress.</summary>
internal static class ShopAnnouncer
{
    private const string BalanceTopic = "shop.balance";
    private const string StatusTopic = "shop.status";
    private const string ObjectivesTopic = "objectives.summary";

    public static void OnShopOpened(MimiBucksShopPanel panel)
    {
        if (!panel.IsOpen)
            return;
        AnnounceGate.Reset(BalanceTopic);
        AnnounceGate.Reset(StatusTopic);
        OnBalance(panel);
        OnStatus(panel);
    }

    public static void OnBalance(MimiBucksShopPanel panel)
    {
        var balance = Text(panel.balanceLabel);
        if (panel.IsOpen && AnnounceGate.Changed(BalanceTopic, balance))
            ScreenReader.Say(Strings.Get("shop.balance", balance));
    }

    public static void OnStatus(MimiBucksShopPanel panel)
    {
        var status = Text(panel.statusLabel);
        if (panel.IsOpen && AnnounceGate.Changed(StatusTopic, status))
            ScreenReader.Say(status);
    }

    public static void OnPurchaseResult(PurchaseResult result) =>
        ScreenReader.Say(Strings.Get("shop.purchase." + result), interrupt: true);

    public static void OnPurchaseDeferred() => ScreenReader.Say(Strings.Get("shop.purchase.deferred"), interrupt: true);

    public static void OnRestored(bool ok) =>
        ScreenReader.Say(Strings.Get(ok ? "shop.restore.ok" : "shop.restore.failed"), interrupt: true);

    public static void OnObjectivesOpened(ObjectivesPanel panel)
    {
        if (!panel.IsOpen)
            return;
        AnnounceGate.Reset(ObjectivesTopic);
        OnObjectivesChanged(panel);
    }

    public static void OnObjectivesChanged(ObjectivesPanel panel)
    {
        if (!panel.IsOpen || panel.content == null)
            return;
        var rows = panel.content.GetComponentsInChildren<ObjectiveItem>(false);
        var done = rows.Count(row => row.fill != null && SameColor(row.fill.color, row.done));
        var summary = Strings.Get("shop.objectives.summary", Text(panel.levelLabel), done, rows.Length);
        if (AnnounceGate.Changed(ObjectivesTopic, summary))
            ScreenReader.Say(summary);
    }

    private static string Text(TMP_Text? label) => label == null ? "" : TextCleaner.Clean(UiQueries.ReadText(label));

    private static bool SameColor(Color actual, Color expected)
    {
        // One 8-bit colour step: UI colours round-trip through 8-bit values.
        const float tolerance = 1f / 255f;
        return Math.Abs(actual.r - expected.r) <= tolerance && Math.Abs(actual.g - expected.g) <= tolerance &&
               Math.Abs(actual.b - expected.b) <= tolerance;
    }
}
