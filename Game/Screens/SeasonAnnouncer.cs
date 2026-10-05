using AccessKit.Speech;
using AccessKit.Text;
using AccessKit.Ui;
using Mimick.Core;
using Mimick.UI;
using TMPro;

namespace MimicPartyAccess.Game.Screens;

/// <summary>Season pass: panel summary, selected reward preview, claim and pass purchase results, XP and level ups.</summary>
internal static class SeasonAnnouncer
{
    private const string PreviewTopic = "season.preview";
    private const string XpTopic = "season.xp";

    public static void OnOpened(SeasonPassPanel panel)
    {
        if (!panel.IsOpen)
            return;
        AnnounceGate.Reset(PreviewTopic);
        ScreenReader.Say(Join(". ", Text(panel.seasonLabel), Text(panel.levelLabel), Text(panel.progressLabel)));
    }

    public static void OnPreviewChanged(SeasonPassPanel panel)
    {
        if (!panel.IsOpen)
            return;
        var preview = Join(". ", Text(panel.previewLabel), Text(panel.previewState));
        if (AnnounceGate.Changed(PreviewTopic, preview))
            ScreenReader.Say(Strings.Get("shop.season.preview", preview));
    }

    public static void OnClaimResult(SeasonPassPanel panel, ClaimResult result)
    {
        var text = Strings.Get("shop.claim." + result);
        if (result == ClaimResult.Success && SeasonPass.RewardAt(panel.shownTier, panel.shownPremium)?.label is string label)
            text = Join(": ", text, TextCleaner.Clean(label));
        ScreenReader.Say(text, interrupt: true);
    }

    public static void OnClaimAll() => ScreenReader.Say(Strings.Get("shop.season.claim_all"), interrupt: true);

    public static void OnBuying() => ScreenReader.Say(Strings.Get("shop.season.buying"), interrupt: true);

    public static void OnPassPurchased() => ScreenReader.Say(Strings.Get("shop.season.pass_active"), interrupt: true);

    public static void OnCelebrate(int bucks) => ScreenReader.Say(Strings.Get("shop.season.celebrate", bucks));

    public static void OnLevelUp() => ScreenReader.Say(Strings.Get("shop.season.level_up", SeasonPass.Level), interrupt: true);

    public static void OnXpShown(SeasonXpBar bar)
    {
        var text = Join(". ", Text(bar.gainLabel), Text(bar.levelLabel));
        if (AnnounceGate.Changed(XpTopic, text))
            ScreenReader.Say(text);
    }

    private static string Text(TMP_Text? label) => label == null ? "" : TextCleaner.Clean(UiQueries.ReadText(label));

    private static string Join(string separator, params string[] parts) =>
        string.Join(separator, parts.Where(part => part.Length > 0));
}
