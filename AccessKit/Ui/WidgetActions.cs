using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AccessKit.Ui;

/// <summary>Performs a control's own behaviour through uGUI's event handlers, exactly as a click or gamepad would.</summary>
internal static class WidgetActions
{
    /// <summary>Left/right on a slider or a closed dropdown; returns true when the value may have changed.</summary>
    public static bool Adjust(Widget widget, int direction, EventSystem eventSystem)
    {
        switch (widget.Kind)
        {
            case WidgetKind.Slider:
                // OnMove steps by the slider's own increment, so no step size is guessed here.
                widget.Slider!.OnMove(new AxisEventData(eventSystem) { moveDir = direction < 0 ? MoveDirection.Left : MoveDirection.Right });
                return true;
            case WidgetKind.Dropdown when !widget.Dropdown!.IsExpanded:
                var dropdown = widget.Dropdown!;
                var value = Math.Clamp(dropdown.value + direction, 0, dropdown.options.Count - 1);
                if (value != dropdown.value)
                    dropdown.value = value;
                return true;
            default:
                return false;
        }
    }

    /// <summary>Enter on the control; returns true when it changes the control's own value rather than the menu.</summary>
    public static bool Activate(Widget widget, EventSystem eventSystem)
    {
        var submit = new BaseEventData(eventSystem);
        switch (widget.Kind)
        {
            case WidgetKind.Toggle:
                widget.Toggle!.OnSubmit(submit);
                return true;
            case WidgetKind.Dropdown:
                widget.Dropdown!.OnSubmit(submit);
                return false;
            case WidgetKind.TextInput:
                widget.Input!.ActivateInputField();
                return false;
            case WidgetKind.Button:
                var button = widget.Selectable!.TryCast<Button>();
                if (button != null)
                    button.OnSubmit(submit);
                else
                    ModLog.WarningOnce("submit:" + widget.Selectable.GetIl2CppType().Name, $"No submit handler for {widget.Selectable.GetIl2CppType().FullName}.");
                return false;
            default:
                return false;
        }
    }

    /// <summary>Keeps the focused control inside its scroll view so sighted helpers see what is being read.</summary>
    public static void ScrollIntoView(Widget widget)
    {
        var scrollRect = widget.ScrollRect;
        var content = scrollRect?.content;
        var viewport = scrollRect?.viewport ?? scrollRect?.transform.TryCast<RectTransform>();
        var item = widget.Target.transform.TryCast<RectTransform>();
        if (content == null || viewport == null || item == null)
            return;

        var itemRect = UiQueries.GetScreenRect(item, widget.Canvas);
        var viewRect = UiQueries.GetScreenRect(viewport, widget.Canvas);
        var shift = itemRect.YMax > viewRect.YMax ? viewRect.YMax - itemRect.YMax
            : itemRect.YMin < viewRect.YMin ? viewRect.YMin - itemRect.YMin
            : 0f;
        if (shift == 0f)
            return;

        var scale = widget.Canvas.scaleFactor > 0f ? widget.Canvas.scaleFactor : 1f;
        var position = content.anchoredPosition;
        content.anchoredPosition = new Vector2(position.x, position.y + shift / scale);
    }
}
