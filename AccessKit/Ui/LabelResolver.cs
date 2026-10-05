using UnityEngine;
using UnityEngine.UI;

namespace AccessKit.Ui;

/// <summary>
/// Finds the static texts that belong to a control: those in the largest ancestor holding no other control, or,
/// when labels and controls are siblings in one list, those nearest to it. Split into the texts read before the
/// control (its label) and after it (its hint/description).
/// </summary>
internal static class LabelResolver
{
    public static (List<string> Before, List<string> After) Resolve(Widget widget)
    {
        var before = new List<string>();
        var after = new List<string>();
        foreach (var (text, rect) in AssociatedTexts(widget))
            (LabelAssignment.IsBefore(rect, widget.Rect) ? before : after).Add(text);
        return (before, after);
    }

    private static IEnumerable<(string Text, ScreenRect Rect)> AssociatedTexts(Widget widget)
    {
        if (widget.Kind == WidgetKind.Info)
            return Enumerable.Empty<(string, ScreenRect)>();
        var container = ExclusiveContainer(widget);
        if (container != null)
            return StaticTexts(container, widget);

        // Texts placed straight on the canvas are screen decoration (version labels, logos), not labels.
        var parent = widget.Target.transform.parent;
        if (parent == null || parent.Pointer == widget.Canvas.transform.Pointer)
            return Enumerable.Empty<(string, ScreenRect)>();

        var controls = parent.GetComponentsInChildren<Selectable>(false).Where(selectable => selectable.IsActive()).ToList();
        var controlRects = controls.Select(selectable => UiQueries.GetScreenRect(selectable)).ToList();
        var self = controls.FindIndex(selectable => selectable.Pointer == widget.Id);
        if (self < 0)
            return Enumerable.Empty<(string, ScreenRect)>();

        return StaticTexts(parent, widget).Where(entry => LabelAssignment.NearestControl(entry.Rect, controlRects) == self).ToList();
    }

    // Texts of a scroll view the control is not part of are list content, never its label.
    private static IEnumerable<(string Text, ScreenRect Rect)> StaticTexts(Transform root, Widget widget)
    {
        var scrollId = widget.ScrollRect?.Pointer ?? IntPtr.Zero;
        return UiQueries.GetVisibleTexts(root)
            .Where(text => text.GetComponentInParent<Selectable>() == null)
            .Where(text => (text.GetComponentInParent<ScrollRect>()?.Pointer ?? IntPtr.Zero) == scrollId)
            .Select(text => (Text: UiQueries.ReadText(text), Rect: UiQueries.GetScreenRect(text)))
            .Where(entry => entry.Text.Length > 0);
    }

    // The largest ancestor below the sorting canvas that contains this control and no other interactive control.
    private static Transform? ExclusiveContainer(Widget widget)
    {
        Transform? container = null;
        var canvasTransform = widget.Canvas.transform;
        for (var parent = widget.Target.transform.parent; parent != null && parent.Pointer != canvasTransform.Pointer; parent = parent.parent)
        {
            var controls = parent.GetComponentsInChildren<Selectable>(false).Count(selectable => selectable.IsActive());
            if (controls > 1)
                break;
            container = parent;
        }
        return container;
    }
}
