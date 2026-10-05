using AccessKit.Text;
using AccessKit.Ui;
using Mimick.Core;
using Mimick.UI;
using UnityEngine.UI;

namespace MimicPartyAccess.Game.Screens;

/// <summary>Season pass reward cells: the buttons have no text, so level, track, reward and state come from game data.</summary>
internal static class SeasonReaders
{
    public static string? NameOf(Selectable selectable)
    {
        if (Cell(selectable) is not var (tier, premium))
            return null;
        var track = Strings.Get(premium ? "shop.track.premium" : "shop.track.free");
        var reward = TextCleaner.Clean(SeasonPass.RewardAt(tier.Tier, premium)?.label);
        return reward.Length == 0
            ? Strings.Get("shop.season.cell_empty", tier.Tier, track)
            : Strings.Get("shop.season.cell", tier.Tier, track, reward);
    }

    public static string? StateOf(Selectable selectable)
    {
        if (Cell(selectable) is not var (tier, premium) || SeasonPass.RewardAt(tier.Tier, premium) is not { } reward)
            return null;
        var key = reward.claimed ? "claimed"
            : SeasonPass.CanClaim(tier.Tier, premium) ? "claimable"
            : premium && !SeasonPass.Premium ? "needs_pass"
            : "locked";
        return Strings.Get("shop.season.state." + key);
    }

    private static (SeasonTierUI Tier, bool Premium)? Cell(Selectable selectable)
    {
        var tier = selectable.GetComponentInParent<SeasonTierUI>();
        if (tier == null)
            return null;
        if (UiQueries.IsSameObject(tier.premiumButton, selectable))
            return (tier, true);
        return UiQueries.IsSameObject(tier.freeButton, selectable) ? (tier, false) : null;
    }
}
