using AccessKit.Text;
using UnityEngine.UI;

namespace MimicPartyAccess.Game.Screens;

/// <summary>Remembers the price of each cosmetic's buy tag so the tag and the cell it sits on can speak it.</summary>
internal static class PriceTags
{
    private static readonly Dictionary<int, (Button Tag, int Price)> ByControl = new();

    public static void Record(Button tag, int price)
    {
        ByControl[tag.GetInstanceID()] = (tag, price);
        // The cell's own button is the nearest selectable above the tag.
        var cell = tag.transform.parent == null ? null : tag.transform.parent.GetComponentInParent<Selectable>();
        if (cell != null)
            ByControl[cell.GetInstanceID()] = (tag, price);
    }

    public static string? StateOf(Selectable selectable)
    {
        if (!ByControl.TryGetValue(selectable.GetInstanceID(), out var entry))
            return null;
        // A bought cosmetic hides or destroys its tag; stop speaking a price then.
        if (entry.Tag == null || !entry.Tag.gameObject.activeInHierarchy)
        {
            ByControl.Remove(selectable.GetInstanceID());
            return null;
        }
        return Strings.Get("shop.price.cost", entry.Price);
    }
}
