using AccessKit.Text;
using AccessKit.Ui;
using Mimick.UI;
using TMPro;
using UnityEngine.UI;

namespace MimicPartyAccess.Game.Screens;

/// <summary>MimiBucks pack rows: the buy button is only a price, so its name joins pack, amount, bonus and price.</summary>
internal static class ShopReaders
{
    public static string? NameOf(Selectable selectable)
    {
        var row = selectable.GetComponentInParent<MimiBucksPackRow>();
        if (row == null || !UiQueries.IsSameObject(row.buyButton, selectable))
            return null;

        var bonus = row.bonusBadge != null && row.bonusBadge.activeInHierarchy ? Text(row.bonusLabel) : "";
        var parts = new[]
        {
            Text(row.nameLabel), Text(row.amountLabel),
            bonus.Length == 0 ? "" : Strings.Get("shop.pack.bonus", bonus), Text(row.buyLabel),
        };
        return Strings.Get("shop.pack.buy", string.Join(", ", parts.Where(part => part.Length > 0)));
    }

    private static string Text(TMP_Text? label) => label == null ? "" : TextCleaner.Clean(UiQueries.ReadText(label));
}
