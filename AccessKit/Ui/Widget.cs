using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AccessKit.Ui;

/// <summary>Widget archetypes from reference/ui-accessibility/generic-strategy.md; every reader keys on these, never on screens.</summary>
public enum WidgetKind
{
    Button,
    Toggle,
    Slider,
    Dropdown,
    TextInput,
    /// <summary>A read-only row of a list (e.g. a friend's name and status) with no control of its own.</summary>
    Info,
}

/// <summary>A navigable item of the active layer: an interactive control, or an information row of a list.</summary>
internal sealed class Widget
{
    public Widget(Component target, Selectable? selectable, WidgetKind kind, Canvas canvas, ScreenRect rect, ScrollRect? scrollRect)
    {
        Target = target;
        Selectable = selectable;
        Kind = kind;
        Canvas = canvas;
        Rect = rect;
        ScrollRect = scrollRect;
    }

    public Component Target { get; }
    public Selectable? Selectable { get; }
    public WidgetKind Kind { get; }
    public Canvas Canvas { get; }
    public ScreenRect Rect { get; }
    public ScrollRect? ScrollRect { get; }
    public IntPtr Id => Target.Pointer;
    public GameObject GameObject => Target.gameObject;

    public TMP_Dropdown? Dropdown => Selectable?.TryCast<TMP_Dropdown>();
    public TMP_InputField? Input => Selectable?.TryCast<TMP_InputField>();
    public Slider? Slider => Selectable?.TryCast<Slider>();
    public Toggle? Toggle => Selectable?.TryCast<Toggle>();
}

internal static class WidgetClassifier
{
    /// <summary>Maps a uGUI selectable onto an archetype; null for controls a player never needs to focus (scrollbars).</summary>
    public static WidgetKind? Classify(Selectable selectable)
    {
        if (selectable.TryCast<TMP_Dropdown>() != null)
            return WidgetKind.Dropdown;
        if (selectable.TryCast<TMP_InputField>() != null)
            return WidgetKind.TextInput;
        if (selectable.TryCast<Slider>() != null)
            return WidgetKind.Slider;
        if (selectable.TryCast<Toggle>() != null)
            return WidgetKind.Toggle;
        if (selectable.TryCast<Scrollbar>() != null)
            return null;
        return WidgetKind.Button;
    }
}
