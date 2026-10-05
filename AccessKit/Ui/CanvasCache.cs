using Il2CppInterop.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AccessKit.Ui;

/// <summary>
/// Scene-wide UI lookups that are too expensive per scan: the sorting canvases, their full-screen graphics
/// (backdrops that block clicks to everything drawn below them) and their scroll views. Refreshed on an
/// interval or on demand.
/// </summary>
internal static class CanvasCache
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(2);
    private static readonly List<(Canvas Canvas, List<Graphic> FullScreen, List<ScrollRect> ScrollViews)> Entries = new();
    private static DateTime _nextRefresh;
    private static int _sceneHandle;

    public static int Generation { get; private set; }

    public static IReadOnlyList<(Canvas Canvas, List<Graphic> FullScreen, List<ScrollRect> ScrollViews)> Get()
    {
        var sceneHandle = SceneManager.GetActiveScene().handle;
        if (DateTime.UtcNow >= _nextRefresh || sceneHandle != _sceneHandle)
            Refresh(sceneHandle);
        return Entries;
    }

    /// <summary>Forces a refresh on the next query (after actions that may create canvases, e.g. opening a dropdown).</summary>
    public static void Invalidate() => _nextRefresh = DateTime.MinValue;

    private static void Refresh(int sceneHandle)
    {
        _nextRefresh = DateTime.UtcNow + RefreshInterval;
        _sceneHandle = sceneHandle;
        Generation++;
        Entries.Clear();

        var screen = UiQueries.ScreenBounds;
        foreach (var obj in UnityEngine.Object.FindObjectsByType(Il2CppType.Of<Canvas>(), FindObjectsSortMode.None))
        {
            var canvas = obj.TryCast<Canvas>();
            if (canvas == null || (!canvas.isRootCanvas && !canvas.overrideSorting))
                continue;

            var fullScreen = new List<Graphic>();
            foreach (var graphic in canvas.GetComponentsInChildren<Graphic>(true))
                if (graphic.raycastTarget && UiQueries.GetSortingCanvas(graphic)?.Pointer == canvas.Pointer &&
                    UiQueries.GetScreenRect(graphic).CoversScreen(screen))
                    fullScreen.Add(graphic);
            var scrollViews = canvas.GetComponentsInChildren<ScrollRect>(true)
                .Where(scrollRect => UiQueries.GetSortingCanvas(scrollRect)?.Pointer == canvas.Pointer)
                .ToList();
            Entries.Add((canvas, fullScreen, scrollViews));
        }
    }
}
