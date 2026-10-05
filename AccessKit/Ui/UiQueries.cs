using System.Text;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AccessKit.Text;

namespace AccessKit.Ui;

/// <summary>Engine-facing queries about uGUI objects: geometry, sorting canvases, visibility and text.</summary>
internal static class UiQueries
{
    private static readonly Il2CppStructArray<Vector3> Corners = new(4);

    public static ScreenRect ScreenBounds => new(0f, 0f, Screen.width, Screen.height);

    public static ScreenRect GetScreenRect(RectTransform rectTransform, Canvas? canvas)
    {
        rectTransform.GetWorldCorners(Corners);
        var camera = canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera ?? Camera.main;
        float xMin = float.MaxValue, yMin = float.MaxValue, xMax = float.MinValue, yMax = float.MinValue;
        for (var i = 0; i < 4; i++)
        {
            var point = camera == null
                ? new Vector2(Corners[i].x, Corners[i].y)
                : RectTransformUtility.WorldToScreenPoint(camera, Corners[i]);
            xMin = Math.Min(xMin, point.x);
            yMin = Math.Min(yMin, point.y);
            xMax = Math.Max(xMax, point.x);
            yMax = Math.Max(yMax, point.y);
        }
        return new ScreenRect(xMin, yMin, xMax, yMax);
    }

    public static ScreenRect GetScreenRect(Component component)
    {
        var rectTransform = component.transform.TryCast<RectTransform>();
        return rectTransform == null ? default : GetScreenRect(rectTransform, GetSortingCanvas(component));
    }

    /// <summary>The canvas that decides draw order for this object: the nearest root or override-sorting canvas.</summary>
    public static Canvas? GetSortingCanvas(Component component)
    {
        var canvas = component.GetComponentInParent<Canvas>();
        while (canvas != null && !canvas.isRootCanvas && !canvas.overrideSorting)
        {
            var parent = canvas.transform.parent;
            canvas = parent == null ? null : parent.GetComponentInParent<Canvas>();
        }
        return canvas;
    }

    /// <summary>Alpha after every parent CanvasGroup is applied; 0 means invisible to a sighted player.</summary>
    public static float GetVisibleAlpha(Graphic? graphic)
    {
        if (graphic == null || !graphic.isActiveAndEnabled)
            return 0f;
        return graphic.canvasRenderer.GetInheritedAlpha() * graphic.color.a;
    }

    /// <summary>Whether CanvasGroups above this object let pointer raycasts reach it.</summary>
    public static bool GroupsAllowRaycasts(Component component)
    {
        for (var transform = component.transform; transform != null; transform = transform.parent)
        {
            var group = transform.GetComponent<CanvasGroup>();
            if (group == null || !group.enabled)
                continue;
            if (!group.blocksRaycasts)
                return false;
            if (group.ignoreParentGroups)
                return true;
        }
        return true;
    }

    public static string ReadText(Component? textComponent)
    {
        var tmp = textComponent?.TryCast<TMP_Text>();
        if (tmp != null)
            return TextCleaner.Clean(tmp.text);
        var legacy = textComponent?.TryCast<UnityEngine.UI.Text>();
        return legacy != null ? TextCleaner.Clean(legacy.text) : "";
    }

    /// <summary>Visible, non-empty TextMeshPro texts under a root, in hierarchy order.</summary>
    public static List<TMP_Text> GetVisibleTexts(Component root)
    {
        var result = new List<TMP_Text>();
        foreach (var text in root.GetComponentsInChildren<TMP_Text>(false))
            if (GetVisibleAlpha(text) > 0f && !string.IsNullOrWhiteSpace(text.text))
                result.Add(text);
        return result;
    }

    public static bool IsSameObject(Il2CppSystem.Object? first, Il2CppSystem.Object? second) =>
        first != null && second != null && first.Pointer == second.Pointer;

    /// <summary>The first game-defined MonoBehaviour on the object or its parents (engine and library types skipped).</summary>
    public static MonoBehaviour? FindGameComponent(Transform? start)
    {
        for (var transform = start; transform != null; transform = transform.parent)
            foreach (var behaviour in transform.GetComponents<MonoBehaviour>())
                if (!IsEngineNamespace(behaviour.GetIl2CppType().Namespace))
                    return behaviour;
        return null;
    }

    public static string? FindGameComponentName(Transform? start) => FindGameComponent(start)?.GetIl2CppType().Name;

    public static bool IsGameComponent(MonoBehaviour behaviour) => !IsEngineNamespace(behaviour.GetIl2CppType().Namespace);

    public static string GetPath(Transform transform, Transform? stopAt = null)
    {
        var builder = new StringBuilder(transform.name);
        for (var parent = transform.parent; parent != null && (stopAt == null || parent.Pointer != stopAt.Pointer); parent = parent.parent)
            builder.Insert(0, parent.name + "/");
        return builder.ToString();
    }

    private static bool IsEngineNamespace(string? ns) =>
        string.IsNullOrEmpty(ns) || ns.StartsWith("UnityEngine", StringComparison.Ordinal) ||
        ns.StartsWith("Unity.", StringComparison.Ordinal) || ns.StartsWith("TMPro", StringComparison.Ordinal) ||
        ns.StartsWith("DG.", StringComparison.Ordinal);
}
