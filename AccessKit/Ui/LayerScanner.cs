using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AccessKit.Ui;

/// <summary>A panel inside the active layer: the widgets of one sorting canvas, in reading order.</summary>
internal sealed record LayerRegion(IntPtr CanvasId, MonoBehaviour? Owner, int SortingOrder, List<Widget> Widgets);

/// <summary>The topmost interactive layer: what a sighted player can click or read right now, in reading order.</summary>
internal sealed class ActiveLayer
{
    public ActiveLayer(string key, Canvas? overlayCanvas, Graphic? blocker, MonoBehaviour? owner, List<LayerRegion> regions, TMP_Dropdown? expandedDropdown)
    {
        Key = key;
        OverlayCanvas = overlayCanvas;
        Blocker = blocker;
        Owner = owner;
        Regions = regions;
        Widgets = regions.SelectMany(region => region.Widgets).ToList();
        ExpandedDropdown = expandedDropdown;
    }

    /// <summary>Stable identity used to detect menu changes and to remember focus per menu.</summary>
    public string Key { get; }
    public Canvas? OverlayCanvas { get; }
    public Graphic? Blocker { get; }
    public MonoBehaviour? Owner { get; }
    public List<LayerRegion> Regions { get; }
    public List<Widget> Widgets { get; }
    public TMP_Dropdown? ExpandedDropdown { get; }
    public bool IsOverlay => OverlayCanvas != null;
    public bool IsDropdownList => ExpandedDropdown != null;

    public int IndexOf(IntPtr id) => Widgets.FindIndex(widget => widget.Id == id);
}

/// <summary>
/// Resolves the active layer generically: every visible, interactable control not covered by a full-screen
/// click blocker drawn above it, plus read-only list rows. The topmost blocker identifies the overlay; with
/// none, it is the base screen. Each sorting canvas is a region read as one block.
/// </summary>
internal static class LayerScanner
{
    // Animated controls (bouncing buttons, entry slides) move every frame; the reading order is kept while the
    // set of items is unchanged so focus never jumps, and recomputed when it changes or on Invalidate.
    private static string _orderSignature = "";
    private static List<IntPtr> _cachedOrder = new();

    public static void InvalidateOrder() => _orderSignature = "";

    public static ActiveLayer Scan(IUiProfile profile, string baseScreenKey)
    {
        var screen = UiQueries.ScreenBounds;
        var candidates = new List<Widget>();
        TMP_Dropdown? expandedDropdown = null;

        foreach (var selectable in Selectable.allSelectablesArray)
        {
            if (selectable == null || !IsUsable(selectable) || WidgetClassifier.Classify(selectable) is not WidgetKind kind)
                continue;

            var canvas = UiQueries.GetSortingCanvas(selectable);
            var rectTransform = selectable.transform.TryCast<RectTransform>();
            if (canvas == null || !canvas.isActiveAndEnabled || rectTransform == null)
                continue;

            var rect = UiQueries.GetScreenRect(rectTransform, canvas);
            var scrollRect = selectable.GetComponentInParent<ScrollRect>();
            var onScreen = scrollRect == null ? rect.Intersects(screen) : IsOnScreen(scrollRect, canvas);
            if (rect.IsEmpty || rect.CoversScreen(screen) || !onScreen)
                continue;

            if (kind == WidgetKind.Dropdown && selectable.TryCast<TMP_Dropdown>()!.IsExpanded)
                expandedDropdown = selectable.TryCast<TMP_Dropdown>();
            candidates.Add(new Widget(selectable, selectable, kind, canvas, rect, scrollRect));
        }
        candidates.AddRange(InfoRows());

        var (blocker, blockerCanvas) = FindTopBlocker();
        var widgets = blocker == null
            ? candidates
            : candidates.Where(widget => DrawOrder.IsDrawnAbove(widget.Target.transform, widget.Canvas, blocker.transform, blockerCanvas!)).ToList();

        // Several overlays can share one canvas, so the blocker (not the canvas) identifies the overlay.
        var owner = blocker == null ? null : UiQueries.FindGameComponent(blocker.transform);
        var key = blocker == null
            ? $"screen:{baseScreenKey}"
            : $"overlay:{owner?.GetIl2CppType().Name ?? blocker.gameObject.name}:{blocker.Pointer}";
        return new ActiveLayer(key, blockerCanvas, blocker, owner, StableRegions(key, widgets), expandedDropdown);
    }

    // A scroll view counts while its viewport is on screen; its content may overflow (that is what scrolling is for).
    private static bool IsOnScreen(ScrollRect scrollRect, Canvas canvas)
    {
        var viewport = scrollRect.viewport ?? scrollRect.transform.TryCast<RectTransform>();
        return viewport != null && UiQueries.GetScreenRect(viewport, canvas).Intersects(UiQueries.ScreenBounds);
    }

    private static bool IsUsable(Selectable selectable) =>
        selectable.IsActive() && selectable.IsInteractable() &&
        (selectable.targetGraphic == null || UiQueries.GetVisibleAlpha(selectable.targetGraphic) > 0f) &&
        UiQueries.GroupsAllowRaycasts(selectable);

    // Rows of a visible list that carry text but no usable control (a friend's name and status): readable items.
    // A row is a container; a bare text placed straight in the list is a label of a neighbouring control.
    private static IEnumerable<Widget> InfoRows()
    {
        foreach (var (canvas, _, scrollViews) in CanvasCache.Get())
        {
            if (canvas == null || !canvas.isActiveAndEnabled)
                continue;
            foreach (var scrollRect in scrollViews)
            {
                var content = scrollRect == null || !scrollRect.isActiveAndEnabled ? null : scrollRect.content;
                if (content == null || !UiQueries.GroupsAllowRaycasts(scrollRect!) || !IsOnScreen(scrollRect!, canvas))
                    continue;
                for (var index = 0; index < content.childCount; index++)
                {
                    var row = content.GetChild(index).TryCast<RectTransform>();
                    if (row == null || !row.gameObject.activeInHierarchy || row.GetComponent<TMP_Text>() != null)
                        continue;
                    if (row.GetComponentsInChildren<Selectable>(false).Any(IsUsable) || UiQueries.GetVisibleTexts(row).Count == 0)
                        continue;
                    yield return new Widget(row, null, WidgetKind.Info, canvas, UiQueries.GetScreenRect(row, canvas), scrollRect);
                }
            }
        }
    }

    private static (Graphic? Blocker, Canvas? Canvas) FindTopBlocker()
    {
        Graphic? top = null;
        Canvas? topCanvas = null;
        foreach (var (canvas, fullScreen, _) in CanvasCache.Get())
        {
            if (canvas == null || !canvas.isActiveAndEnabled)
                continue;
            foreach (var graphic in fullScreen)
            {
                if (graphic == null || !graphic.isActiveAndEnabled || !graphic.raycastTarget || !UiQueries.GroupsAllowRaycasts(graphic))
                    continue;
                if (top == null || DrawOrder.IsDrawnAbove(graphic.transform, canvas, top.transform, topCanvas!))
                {
                    top = graphic;
                    topCanvas = canvas;
                }
            }
        }
        return (top, topCanvas);
    }

    // Regions (sorting canvases) are read bottom panel first, so the base screen comes before panels drawn over it;
    // among canvases at the same order the one with the most items (the screen itself) leads.
    private static List<LayerRegion> StableRegions(string key, List<Widget> widgets)
    {
        var signature = key + "|" + string.Join(",", widgets.Select(widget => widget.Id.ToInt64()).OrderBy(id => id));
        if (signature != _orderSignature)
        {
            _orderSignature = signature;
            _cachedOrder = widgets
                .GroupBy(widget => widget.Canvas.Pointer)
                .OrderBy(group => group.First().Canvas.sortingOrder)
                .ThenByDescending(group => group.Count())
                .SelectMany(group => Order(group.ToList()))
                .Select(widget => widget.Id)
                .ToList();
        }

        var byId = widgets.ToDictionary(widget => widget.Id);
        return _cachedOrder
            .Select(id => byId[id])
            .GroupBy(widget => widget.Canvas.Pointer)
            .Select(group => new LayerRegion(group.Key, UiQueries.FindGameComponent(group.First().Canvas.transform), group.First().Canvas.sortingOrder, group.ToList()))
            .ToList();
    }

    private static List<Widget> Order(List<Widget> widgets)
    {
        var scrollIds = new Dictionary<IntPtr, int>();
        var viewports = new Dictionary<int, ScreenRect>();
        var groups = new List<int?>();
        foreach (var widget in widgets)
        {
            if (widget.ScrollRect == null)
            {
                groups.Add(null);
                continue;
            }

            if (!scrollIds.TryGetValue(widget.ScrollRect.Pointer, out var id))
            {
                id = scrollIds.Count;
                scrollIds[widget.ScrollRect.Pointer] = id;
                var viewport = widget.ScrollRect.viewport ?? widget.ScrollRect.transform.TryCast<RectTransform>();
                viewports[id] = viewport == null ? widget.Rect : UiQueries.GetScreenRect(viewport, widget.Canvas);
            }
            groups.Add(id);
        }

        return VisualOrder.Order(widgets.Select(widget => widget.Rect).ToList(), groups, viewports)
            .Select(index => widgets[index])
            .ToList();
    }
}
