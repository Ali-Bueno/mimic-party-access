using System.Text;
using Il2CppInterop.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using AccessKit.Input;

namespace AccessKit.Ui;

/// <summary>
/// Development aid: writes the live uGUI structure (sorting canvases, selectables, blockers, texts with screen
/// rects) to ui-dump.txt, on Ctrl+Shift+F12 and whenever the set of interactive canvases changes.
/// </summary>
public static class UiDiagnostics
{
    private const int MaxTextsPerCanvas = 60;
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(250);

    private static string _dumpPath = "";
    private static string _lastSignature = "";
    private static DateTime _nextPoll;

    public static void Initialize(string pluginDirectory)
    {
        _dumpPath = Path.Combine(pluginDirectory, "ui-dump.txt");
        File.WriteAllText(_dumpPath, "");
    }

    public static void Tick()
    {
        if (!ModConfig.UiDump)
            return;
        if (KeyInput.CtrlHeld && KeyInput.ShiftHeld && KeyInput.Down(KeyCode.F12))
            Dump("manual");
        if (DateTime.UtcNow < _nextPoll)
            return;

        _nextPoll = DateTime.UtcNow + PollInterval;
        var signature = BuildSignature();
        if (signature == _lastSignature)
            return;
        _lastSignature = signature;
        Dump("interactive canvases changed: " + signature);
    }

    public static void Dump(string reason)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"=== {DateTime.Now:HH:mm:ss.fff} {reason} | screen {Screen.width}x{Screen.height}");
        var eventSystem = EventSystem.current;
        builder.AppendLine(eventSystem == null
            ? "EventSystem: none"
            : $"EventSystem: {eventSystem.name} selected={Describe(eventSystem.currentSelectedGameObject)} module={eventSystem.currentInputModule?.GetIl2CppType().FullName}");

        var selectablesByCanvas = Selectable.allSelectablesArray
            .Where(selectable => selectable != null)
            .GroupBy(selectable => UiQueries.GetSortingCanvas(selectable)?.Pointer ?? IntPtr.Zero)
            .ToDictionary(group => group.Key, group => group.ToList());

        foreach (var obj in UnityEngine.Object.FindObjectsByType(Il2CppType.Of<Canvas>(), FindObjectsSortMode.None))
        {
            var canvas = obj.TryCast<Canvas>();
            if (canvas == null || !canvas.isActiveAndEnabled || (!canvas.isRootCanvas && !canvas.overrideSorting))
                continue;
            DumpCanvas(builder, canvas, selectablesByCanvas.TryGetValue(canvas.Pointer, out var list) ? list : new List<Selectable>());
        }

        File.AppendAllText(_dumpPath, builder.ToString());
        ModLog.Info($"UI dump written ({reason}).");
    }

    private static void DumpCanvas(StringBuilder builder, Canvas canvas, List<Selectable> selectables)
    {
        var transform = canvas.transform;
        builder.AppendLine($"CANVAS {UiQueries.GetPath(transform)} order={canvas.sortingOrder} layer={canvas.sortingLayerName} root={canvas.isRootCanvas} mode={canvas.renderMode} game={UiQueries.FindGameComponentName(transform)} group={DescribeGroups(transform)}");

        foreach (var graphic in canvas.GetComponentsInChildren<Graphic>(false))
        {
            if (!graphic.raycastTarget || UiQueries.GetSortingCanvas(graphic)?.Pointer != canvas.Pointer)
                continue;
            var rect = UiQueries.GetScreenRect(graphic);
            if (rect.CoversScreen(UiQueries.ScreenBounds))
                builder.AppendLine($"  BLOCKER {UiQueries.GetPath(graphic.transform, transform)} type={graphic.GetIl2CppType().Name} alpha={UiQueries.GetVisibleAlpha(graphic):0.##} raycasts={UiQueries.GroupsAllowRaycasts(graphic)}");
        }

        foreach (var selectable in selectables)
        {
            var texts = string.Join(" | ", UiQueries.GetVisibleTexts(selectable).Select(text => UiQueries.ReadText(text)));
            builder.AppendLine($"  SEL {selectable.GetIl2CppType().Name} {UiQueries.GetPath(selectable.transform, transform)} active={selectable.IsActive()} interactable={selectable.IsInteractable()} alpha={UiQueries.GetVisibleAlpha(selectable.targetGraphic):0.##} raycasts={UiQueries.GroupsAllowRaycasts(selectable)} rect={UiQueries.GetScreenRect(selectable)} game={UiQueries.FindGameComponentName(selectable.transform)} text=\"{texts}\"");
        }

        var count = 0;
        foreach (var text in UiQueries.GetVisibleTexts(canvas))
        {
            if (text.GetComponentInParent<Selectable>() != null || UiQueries.GetSortingCanvas(text)?.Pointer != canvas.Pointer)
                continue;
            if (++count > MaxTextsPerCanvas)
                break;
            builder.AppendLine($"  TXT {UiQueries.GetPath(text.transform, transform)} size={text.fontSize:0} rect={UiQueries.GetScreenRect(text)} \"{UiQueries.ReadText(text)}\"");
        }
    }

    private static string BuildSignature()
    {
        var parts = Selectable.allSelectablesArray
            .Where(selectable => selectable != null && selectable.IsInteractable() && UiQueries.GetVisibleAlpha(selectable.targetGraphic) > 0f &&
                                 UiQueries.GetScreenRect(selectable).Intersects(UiQueries.ScreenBounds))
            .GroupBy(selectable => UiQueries.GetSortingCanvas(selectable)?.name ?? "-")
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => $"{group.Key}:{group.Count()}");
        return string.Join(",", parts);
    }

    private static string DescribeGroups(Transform transform)
    {
        var parts = new List<string>();
        for (var current = transform; current != null; current = current.parent)
        {
            var group = current.GetComponent<CanvasGroup>();
            if (group != null)
                parts.Add($"{current.name}(a={group.alpha:0.##},i={group.interactable},r={group.blocksRaycasts})");
        }
        return parts.Count == 0 ? "-" : string.Join(">", parts);
    }

    private static string Describe(GameObject? gameObject) => gameObject == null ? "none" : UiQueries.GetPath(gameObject.transform);
}
