using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AccessKit.Text;

namespace AccessKit.Ui;

/// <summary>
/// Archetype readers (reference/ui-accessibility/menus.md): focus reads name, then state/value; a value change
/// reads only the new value. The name is the control's label (LabelResolver) plus its own text, with the strings
/// layer as fallback; texts that follow the control are read last as its description.
/// </summary>
internal static class WidgetReader
{
    public static IUiProfile Profile { get; set; } = new DefaultUiProfile();

    public static string Describe(Widget widget)
    {
        var name = Name(widget);
        var value = Value(widget);
        var parts = new List<string> { name };
        if (!string.IsNullOrEmpty(value) && !name.Contains(value, StringComparison.OrdinalIgnoreCase))
            parts.Add(value);
        parts.AddRange(LabelResolver.Resolve(widget).After);
        return string.Join(", ", parts);
    }

    /// <summary>Only the current value, for horizontal changes; falls back to the full readout for valueless controls.</summary>
    public static string DescribeValue(Widget widget)
    {
        // Games often print a slider's value in its label ("Music 80 %"); that number is the game's own formatting.
        if (widget.Kind == WidgetKind.Slider && TextCleaner.TrailingQuantity(Name(widget)) is string shown)
            return shown;
        return Value(widget) ?? Describe(widget);
    }

    public static string Name(Widget widget)
    {
        var custom = widget.Selectable == null ? null : Profile.GetWidgetName(widget.Selectable);
        if (!string.IsNullOrWhiteSpace(custom))
            return WithRowTitle(widget, custom);

        var own = string.Join(", ", OwnTexts(widget));
        var name = string.Join(", ", LabelResolver.Resolve(widget).Before.Append(own).Where(part => part.Length > 0));

        var objectName = widget.GameObject.name;
        if (Strings.TryGet("widget." + objectName, out var format))
            name = string.Format(CultureInfo.InvariantCulture, format, name);
        if (name.Length == 0 && widget.Input?.placeholder is Graphic placeholder)
            name = UiQueries.ReadText(placeholder);
        if (name.Length == 0)
            name = TextCleaner.Humanize(objectName);

        return WithRowTitle(widget, name);
    }

    private static string WithRowTitle(Widget widget, string name)
    {
        var row = RowTitle(widget);
        return row == null || name.Contains(row, StringComparison.OrdinalIgnoreCase) ? name : $"{row}, {name}";
    }

    public static string? Value(Widget widget)
    {
        var customState = widget.Selectable == null ? null : Profile.GetWidgetState(widget.Selectable);
        if (!string.IsNullOrEmpty(customState))
            return customState;

        switch (widget.Kind)
        {
            case WidgetKind.Toggle:
                return Strings.Get(widget.Toggle!.isOn ? "ui.on" : "ui.off");
            case WidgetKind.Slider:
                return FormatSlider(widget.Slider!);
            case WidgetKind.Dropdown:
                var dropdown = widget.Dropdown!;
                var caption = UiQueries.ReadText(dropdown.captionText);
                if (caption.Length == 0 && dropdown.value >= 0 && dropdown.value < dropdown.options.Count)
                    caption = TextCleaner.Clean(dropdown.options[dropdown.value].text);
                return caption;
            case WidgetKind.TextInput:
                var input = widget.Input!;
                var text = input.contentType == TMP_InputField.ContentType.Password ? "" : TextCleaner.Clean(input.text);
                return $"{Strings.Get("ui.text_field")}, {(text.Length > 0 ? text : Strings.Get("ui.empty"))}";
            default:
                return null;
        }
    }

    private static string FormatSlider(Slider slider)
    {
        var range = slider.maxValue - slider.minValue;
        if (range <= 0f)
            return slider.value.ToString(CultureInfo.InvariantCulture);
        if (slider.wholeNumbers)
            return Mathf.RoundToInt(slider.value).ToString(CultureInfo.InvariantCulture);
        var percent = Mathf.RoundToInt((slider.value - slider.minValue) / range * 100f);
        return Strings.Get("ui.percent", percent);
    }

    /// <summary>
    /// The title of the list row (or parent control) a control sits in, so "Download" or an icon button says which
    /// item it acts on: the row's largest text outside this control, preferring texts not inside other controls.
    /// </summary>
    private static string? RowTitle(Widget widget)
    {
        var row = RowOf(widget);
        if (row == null)
            return null;

        var own = widget.Target.transform;
        var texts = UiQueries.GetVisibleTexts(row)
            .Where(text => !text.transform.IsChildOf(own))
            .Select(text => (Text: text, Static: text.GetComponentInParent<Selectable>() is not Selectable owner || owner.transform.Pointer == row.Pointer))
            .ToList();
        var pool = texts.Any(entry => entry.Static) ? texts.Where(entry => entry.Static).ToList() : texts;
        if (pool.Count == 0)
            return null;
        var title = UiQueries.ReadText(pool.MaxBy(entry => entry.Text.fontSize).Text);
        return title.Length == 0 ? null : title;
    }

    // A row is the direct child of a scroll view's content that contains the control, a control containing it, or a
    // list item: an ancestor whose siblings all carry the same game script (row prefabs such as pack vote rows).
    private static Transform? RowOf(Widget widget)
    {
        var content = widget.ScrollRect?.content;
        var canvas = widget.Canvas.transform;
        for (var current = widget.Target.transform.parent; current != null && current.Pointer != canvas.Pointer; current = current.parent)
        {
            if (current.GetComponent<Selectable>() is Selectable parentControl && parentControl.IsActive())
                return current;
            if (content != null && current.parent != null && current.parent.Pointer == content.Pointer)
                return current;
            if (content != null && current.Pointer == content.Pointer)
                return null;
            if (IsListItem(current))
                return current;
        }
        return null;
    }

    private static bool IsListItem(Transform transform)
    {
        var script = RowScriptName(transform);
        var parent = transform.parent;
        if (script == null || parent == null)
            return false;
        for (var index = 0; index < parent.childCount; index++)
        {
            var sibling = parent.GetChild(index);
            if (sibling.gameObject.activeSelf && RowScriptName(sibling) != script)
                return false;
        }
        return true;
    }

    // Graphics (rounded images, shadows) are visuals, not the script that defines a row prefab.
    private static string? RowScriptName(Transform transform) =>
        transform.GetComponents<MonoBehaviour>()
            .FirstOrDefault(behaviour => behaviour.TryCast<Graphic>() == null && UiQueries.IsGameComponent(behaviour))
            ?.GetIl2CppType().Name;

    private static IEnumerable<string> OwnTexts(Widget widget)
    {
        var excluded = new HashSet<IntPtr>();
        if (widget.Dropdown is TMP_Dropdown dropdown && dropdown.captionText != null)
            excluded.Add(dropdown.captionText.Pointer);
        if (widget.Input is TMP_InputField input)
        {
            if (input.textComponent != null)
                excluded.Add(input.textComponent.Pointer);
            if (input.placeholder != null)
                excluded.Add(input.placeholder.Pointer);
        }

        return UiQueries.GetVisibleTexts(widget.Target)
            .Where(text => !excluded.Contains(text.Pointer))
            .Select(text => UiQueries.ReadText(text))
            .Where(text => text.Length > 0);
    }
}
