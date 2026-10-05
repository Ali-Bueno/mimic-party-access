using UnityEngine;
using UnityEngine.EventSystems;
using AccessKit.Input;
using AccessKit.Speech;
using AccessKit.Text;

namespace AccessKit.Ui;

/// <summary>
/// Keyboard navigation over the active layer and every menu announcement (reference/ui-accessibility/menus.md).
/// The game's EventSystem keeps the selection (so visuals and clicks stay native) but stops reading arrow keys,
/// so the mod is the only thing that moves focus.
/// </summary>
public static class MenuNavigator
{
    private static readonly TimeSpan ScanInterval = TimeSpan.FromMilliseconds(200);

    // A new layer must be seen on this many consecutive scans before it is announced, so panels that flash
    // through intermediate states while animating in are not read twice.
    private const int StableScansBeforeAnnounce = 2;

    // Entry animations slide controls in after a menu appears; the reading order is recomputed once they settle.
    private static readonly TimeSpan EntryAnimationSettle = TimeSpan.FromSeconds(1);
    private static DateTime? _reorderAt;

    // A collapsed side panel (a drawer) shows at most its own handle; growing past that means it was opened.
    private const int CollapsedPanelControls = 1;
    private static Dictionary<IntPtr, int> _regionSizes = new();
    private static IntPtr _focusCanvasId;
    private static DateTime _regionsSettleAt;

    private static IUiProfile _profile = new DefaultUiProfile();
    private static ActiveLayer? _layer;
    private static string? _announcedKey;
    private static string? _candidateKey;
    private static int _candidateScans;
    private static IntPtr _focusId;
    private static string? _lastReadout;
    private static string _baseScreenKey = "";
    private static int _baseKeyGeneration = -1;
    private static DateTime _nextScan;
    private static (IntPtr Id, int Frame)? _pendingValue;
    private static readonly Dictionary<string, IntPtr> FocusMemory = new();

    public static void Initialize(IUiProfile profile)
    {
        _profile = profile;
        WidgetReader.Profile = profile;
    }

    public static void Tick()
    {
        if (!ModConfig.Menus)
            return;
        var eventSystem = EventSystem.current;
        if (eventSystem == null)
            return;
        if (eventSystem.sendNavigationEvents)
            eventSystem.sendNavigationEvents = false;

        HandleHotkeys();
        if (ChoicePrompt.Tick())
            return;
        if (DateTime.UtcNow >= _nextScan)
            Rescan();
        if (_layer != null && _layer.Key == _announcedKey && !_profile.IsCapturingInput(_layer.Owner))
            HandleNavigationKeys(eventSystem);
        FlushPendingValue();
    }

    private static void HandleHotkeys()
    {
        if (!KeyInput.CtrlHeld || !KeyInput.AltHeld)
            return;
        if (KeyInput.Down(KeyCode.R))
            ScreenReader.Repeat();
        else if (KeyInput.Down(KeyCode.F1))
            ScreenReader.Say(Strings.Get("ui.help"), interrupt: true);
    }

    private static void HandleNavigationKeys(EventSystem eventSystem)
    {
        if (KeyInput.CtrlHeld || KeyInput.AltHeld)
            return;

        var current = Current();
        var editingText = current?.Input is { isFocused: true };
        if (KeyInput.Down(KeyCode.UpArrow))
            Move(-1, eventSystem);
        else if (KeyInput.Down(KeyCode.DownArrow))
            Move(1, eventSystem);
        else if (editingText)
            return;
        else if (KeyInput.Down(KeyCode.Home))
            MoveTo(_ => 0, eventSystem);
        else if (KeyInput.Down(KeyCode.End))
            MoveTo(count => count - 1, eventSystem);
        else if (KeyInput.Down(KeyCode.LeftArrow))
            Adjust(-1, eventSystem);
        else if (KeyInput.Down(KeyCode.RightArrow))
            Adjust(1, eventSystem);
        else if (KeyInput.Down(KeyCode.Return) || KeyInput.Down(KeyCode.KeypadEnter))
            Activate(eventSystem);
    }

    private static void Rescan()
    {
        _nextScan = DateTime.UtcNow + ScanInterval;
        if (_reorderAt is { } reorderAt && DateTime.UtcNow >= reorderAt)
        {
            _reorderAt = null;
            LayerScanner.InvalidateOrder();
        }
        if (_baseKeyGeneration != CanvasCache.Generation)
        {
            _baseKeyGeneration = CanvasCache.Generation;
            _baseScreenKey = _profile.GetBaseScreenKey() ?? "";
        }

        _layer = LayerScanner.Scan(_profile, _baseScreenKey);
        if (_layer.Widgets.Count == 0)
            return;

        if (_layer.Key == _announcedKey)
        {
            if (!TrackRegions(_layer))
                RefreshFocus();
            return;
        }

        _candidateScans = _layer.Key == _candidateKey ? _candidateScans + 1 : 1;
        _candidateKey = _layer.Key;
        if (_candidateScans >= StableScansBeforeAnnounce)
            EnterLayer(_layer);
    }

    private static void EnterLayer(ActiveLayer layer)
    {
        var previousFocus = _focusId;
        var previousKey = _announcedKey;
        if (previousKey != null)
            FocusMemory[previousKey] = _focusId;
        _announcedKey = layer.Key;
        _reorderAt = DateTime.UtcNow + EntryAnimationSettle;
        _regionSizes = RegionSizes(layer);
        _regionsSettleAt = DateTime.UtcNow + EntryAnimationSettle;
        LogUi($"enter {layer.Key} ({layer.Widgets.Count} controls): {string.Join(" | ", layer.Widgets.Select(widget => widget.GameObject.name))}");

        var eventSystem = EventSystem.current;
        var selected = eventSystem?.currentSelectedGameObject;
        var index = selected == null ? -1 : layer.Widgets.FindIndex(widget => widget.GameObject.Pointer == selected.Pointer);
        if (index < 0 && FocusMemory.TryGetValue(layer.Key, out var remembered))
            index = layer.IndexOf(remembered);
        var focus = layer.Widgets[Math.Max(index, 0)];
        Focus(focus, eventSystem);

        var readout = WidgetReader.Describe(focus);
        _lastReadout = readout;

        // The game opened or closed something by itself while focus stayed on the same control (an in-match reveal or
        // wheel over the HUD): the control is not repeated, and going back to the base screen is silent.
        var focusUnchanged = focus.Id == previousFocus;
        if (focusUnchanged && !layer.IsOverlay)
            return;
        if (layer.IsDropdownList)
        {
            ScreenReader.Say(WidgetReader.Name(focus), interrupt: true);
            return;
        }

        var title = LayerDescriber.Title(layer, _profile, _baseScreenKey);
        var body = LayerDescriber.DialogBody(layer, title);
        var parts = new[] { title, body, focusUnchanged ? null : readout }.Where(part => !string.IsNullOrWhiteSpace(part)).Select(part => part!.TrimEnd('.', ' '));
        ScreenReader.Say(string.Join(". ", parts), interrupt: true);
    }

    // A panel that opens over the screen without blocking it (a drawer) is entered like a submenu.
    private static bool TrackRegions(ActiveLayer layer)
    {
        LayerRegion? opened = null;
        foreach (var region in layer.Regions.Skip(1))
        {
            var previous = _regionSizes.TryGetValue(region.CanvasId, out var size) ? size : 0;
            if (previous <= CollapsedPanelControls && region.Widgets.Count > CollapsedPanelControls)
                opened = region;
        }
        _regionSizes = RegionSizes(layer);
        if (opened == null || DateTime.UtcNow < _regionsSettleAt)
            return false;

        var focus = opened.Widgets[0];
        LogUi($"open region {opened.Owner?.GetIl2CppType().Name} ({opened.Widgets.Count}): {string.Join(" | ", opened.Widgets.Select(widget => widget.GameObject.name))}");
        Focus(focus, EventSystem.current);
        _lastReadout = WidgetReader.Describe(focus);
        var title = LayerDescriber.RegionTitle(opened, _profile);
        ScreenReader.Say($"{title.TrimEnd('.', ' ')}. {_lastReadout}", interrupt: true);
        return true;
    }

    private static Dictionary<IntPtr, int> RegionSizes(ActiveLayer layer) =>
        layer.Regions.ToDictionary(region => region.CanvasId, region => region.Widgets.Count);

    // Keeps the readout of the focused control current when the game rebuilds or updates it (diff-gated).
    private static void RefreshFocus()
    {
        var layer = _layer!;
        var current = Current();
        if (current == null)
        {
            // The focused item vanished (a panel closed, a list refreshed): stay in the same panel if it still has items.
            var fallback = layer.Regions.FirstOrDefault(region => region.CanvasId == _focusCanvasId)?.Widgets[0] ?? layer.Widgets[0];
            Focus(fallback, EventSystem.current);
            _lastReadout = WidgetReader.Describe(fallback);
            ScreenReader.Say(_lastReadout);
            return;
        }

        if (_pendingValue != null)
            return;
        var readout = WidgetReader.Describe(current);
        if (readout == _lastReadout)
            return;
        var previous = _lastReadout;
        _lastReadout = readout;
        var name = WidgetReader.Name(current);
        ScreenReader.Say(previous != null && previous.StartsWith(name, StringComparison.Ordinal) && WidgetReader.Value(current) != null
            ? WidgetReader.DescribeValue(current)
            : readout);
    }

    private static void Move(int direction, EventSystem eventSystem)
    {
        MoveTo(count =>
        {
            var index = _layer!.IndexOf(_focusId);
            return index < 0 ? (direction > 0 ? 0 : count - 1) : (index + direction + count) % count;
        }, eventSystem);
    }

    /// <summary>Focuses the item whose index <paramref name="target"/> picks from the item count.</summary>
    private static void MoveTo(Func<int, int> target, EventSystem eventSystem)
    {
        Rescan();
        var layer = _layer;
        if (layer == null || layer.Widgets.Count == 0 || layer.Key != _announcedKey)
            return;

        var next = target(layer.Widgets.Count);
        var widget = layer.Widgets[next];
        LogUi($"move -> {next + 1}/{layer.Widgets.Count} {widget.GameObject.name}");
        Focus(widget, eventSystem);
        _lastReadout = WidgetReader.Describe(widget);
        ScreenReader.Say(layer.IsDropdownList ? WidgetReader.Name(widget) : _lastReadout, interrupt: true);
    }

    private static void Adjust(int direction, EventSystem eventSystem)
    {
        var widget = Current();
        if (widget == null)
            return;
        if (widget.Kind is WidgetKind.Button or WidgetKind.TextInput or WidgetKind.Info)
            Move(direction, eventSystem);
        else if (WidgetActions.Adjust(widget, direction, eventSystem))
            QueueValueRead(widget);
    }

    private static void Activate(EventSystem eventSystem)
    {
        var widget = Current();
        if (widget == null)
            return;

        LogUi($"activate {widget.Kind} {widget.GameObject.name}");
        var valueChanged = WidgetActions.Activate(widget, eventSystem);
        if (valueChanged || WidgetReader.Value(widget) != null)
            QueueValueRead(widget);
        CanvasCache.Invalidate();
    }

    private static void LogUi(string message)
    {
        if (ModConfig.LogUi)
            ModLog.Info("[ui] " + message);
    }

    private static void QueueValueRead(Widget widget) => _pendingValue = (widget.Id, Time.frameCount + 1);

    // Values are read a frame later so the game's own value labels have been updated by its change handlers.
    private static void FlushPendingValue()
    {
        if (_pendingValue is not { } pending || Time.frameCount < pending.Frame)
            return;
        _pendingValue = null;
        var widget = _layer?.Widgets.Find(candidate => candidate.Id == pending.Id);
        if (widget == null)
            return;
        _lastReadout = WidgetReader.Describe(widget);
        ScreenReader.Say(WidgetReader.DescribeValue(widget), interrupt: true);
    }

    private static Widget? Current()
    {
        var layer = _layer;
        if (layer == null)
            return null;
        var index = layer.IndexOf(_focusId);
        return index < 0 ? null : layer.Widgets[index];
    }

    private static void Focus(Widget widget, EventSystem? eventSystem)
    {
        _focusId = widget.Id;
        _focusCanvasId = widget.Canvas.Pointer;
        eventSystem?.SetSelectedGameObject(widget.Selectable == null ? null : widget.GameObject);
        WidgetActions.ScrollIntoView(widget);
    }
}
