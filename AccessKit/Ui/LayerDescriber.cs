using UnityEngine.UI;
using AccessKit.Text;

namespace AccessKit.Ui;

/// <summary>Reads a layer's title and, for dialogs, its message: data read off the screen, strings layer as fallback.</summary>
internal static class LayerDescriber
{
    // A confirm/cancel/close dialog never offers more than this many controls; larger overlays are menus whose
    // static texts are control labels, not a message to read in full.
    private const int MaxDialogControls = 3;

    public static string Title(ActiveLayer layer, IUiProfile profile, string baseScreenKey)
    {
        if (!layer.IsOverlay)
            return NameFor(baseScreenKey);

        var ownerName = layer.Owner?.GetIl2CppType().Name ?? layer.Blocker?.gameObject.name;
        var title = profile.GetOverlayTitle(layer.Owner);
        if (!string.IsNullOrWhiteSpace(title))
            return title;
        if (ownerName != null && Strings.TryGet("layer." + ownerName, out var named))
            return named;
        return LargestText(layer)?.Text ?? NameFor(ownerName ?? layer.OverlayCanvas!.name);
    }

    /// <summary>Title of a panel that opened inside the current layer: the game's own heading, else its strings name.</summary>
    public static string RegionTitle(LayerRegion region, IUiProfile profile)
    {
        var title = profile.GetOverlayTitle(region.Owner);
        if (!string.IsNullOrWhiteSpace(title))
            return title;
        return NameFor(region.Owner?.GetIl2CppType().Name ?? "");
    }

    /// <summary>Full message of a dialog-sized overlay (static texts other than the title), else null.</summary>
    public static string? DialogBody(ActiveLayer layer, string title)
    {
        if (!layer.IsOverlay || layer.Widgets.Count > MaxDialogControls)
            return null;

        var texts = OverlayTexts(layer)
            .Where(entry => entry.Text != title)
            .ToList();
        if (texts.Count == 0)
            return null;

        var order = VisualOrder.OrderRects(texts.Select(entry => entry.Rect).ToList());
        return string.Join(". ", order.Select(index => texts[index].Text.TrimEnd('.', ' ')));
    }

    private static (string Text, ScreenRect Rect, float Size)? LargestText(ActiveLayer layer)
    {
        var texts = OverlayTexts(layer);
        if (texts.Count == 0)
            return null;
        var largest = texts.MaxBy(entry => entry.Size);
        return texts.Count(entry => entry.Size >= largest.Size) == 1 ? largest : null;
    }

    // Visible static texts of the overlay drawn above its blocker and not part of any control.
    private static List<(string Text, ScreenRect Rect, float Size)> OverlayTexts(ActiveLayer layer)
    {
        var canvas = layer.OverlayCanvas!;
        var blocker = layer.Blocker!;
        var result = new List<(string, ScreenRect, float)>();
        foreach (var text in UiQueries.GetVisibleTexts(canvas))
        {
            if (text.GetComponentInParent<Selectable>() != null || UiQueries.GetSortingCanvas(text)?.Pointer != canvas.Pointer)
                continue;
            if (!DrawOrder.IsDrawnAbove(text.transform, canvas, blocker.transform, canvas))
                continue;
            var content = UiQueries.ReadText(text);
            if (content.Length > 0)
                result.Add((content, UiQueries.GetScreenRect(text), text.fontSize));
        }
        return result;
    }

    private static string NameFor(string key)
    {
        return Strings.TryGet("layer." + key, out var named) ? named : TextCleaner.Humanize(key);
    }
}
