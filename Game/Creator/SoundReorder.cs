using AccessKit.Text;
using AccessKit.Ui;
using Mimick.Data;
using Mimick.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MimicPartyAccess.Game.Creator;

/// <summary>
/// Moves the focused sound row one place by replaying the drag the game expects (ReorderBegan, ReorderMoved to the
/// neighbour row, ReorderEnded), then announces the sound's new position once the screen has rebuilt its rows.
/// </summary>
internal static class SoundReorder
{
    // Aim past the neighbour's centre (a quarter of the row spacing) so the game's hit test clearly picks that row.
    private const float OvershootOfRowSpacing = 0.25f;

    private static DraftSound? _moved;
    private static int _slot;
    private static int _indexBefore;
    private static int _frame;

    public static bool TryMove(int direction)
    {
        var focused = CreatorLookup.Selected();
        var row = focused?.GetComponentInParent<SoundRowUI>();
        var screen = CreatorLookup.FindActive<ThemeCreatorScreen>();
        if (focused == null || row == null || screen == null || row.Payload?.TryCast<DraftSound>() is not DraftSound sound)
            return false;

        var rows = CreatorLookup.Rows(screen);
        var position = rows.FindIndex(candidate => UiQueries.IsSameObject(candidate, row));
        if (position < 0)
            return false;
        var target = position + direction;
        if (target < 0 || target >= rows.Count)
        {
            CreatorAnnouncer.Speak(Strings.Get(direction < 0 ? "creator.reorder.first" : "creator.reorder.last"), interrupt: true);
            return true;
        }

        var from = ScreenCenter(rows[position]);
        var to = ScreenCenter(rows[target]);
        _indexBefore = IndexOf(screen, sound);
        var focusedControl = focused.GetComponent<Selectable>();
        _slot = Array.FindIndex(Controls(row), control => control != null && UiQueries.IsSameObject(control, focusedControl));

        screen.ReorderBegan(row);
        try
        {
            screen.ReorderMoved(row, to + (to - from) * OvershootOfRowSpacing);
        }
        finally
        {
            screen.ReorderEnded(row);
        }
        _moved = sound;
        _frame = Time.frameCount;
        return true;
    }

    /// <summary>Announces the result on the frame after the move, restoring focus if the rows were rebuilt.</summary>
    public static void Tick()
    {
        if (_moved == null || Time.frameCount <= _frame)
            return;
        var sound = _moved;
        _moved = null;

        var screen = CreatorLookup.FindActive<ThemeCreatorScreen>();
        if (screen == null)
            return;
        var index = IndexOf(screen, sound);
        if (index < 0 || index == _indexBefore)
        {
            CreatorAnnouncer.Speak(Strings.Get("creator.reorder.failed"), interrupt: true);
            return;
        }

        RestoreFocus(screen, sound);
        var count = screen.draft?.Sounds?.Count ?? 0;
        CreatorAnnouncer.Speak(Strings.Get("creator.reorder.position", index + 1, count), interrupt: true);
    }

    private static void RestoreFocus(ThemeCreatorScreen screen, DraftSound sound)
    {
        var focused = CreatorLookup.Selected();
        if (focused != null && focused.activeInHierarchy)
            return;
        var row = CreatorLookup.Rows(screen).Find(candidate => UiQueries.IsSameObject(candidate.Payload, sound));
        var controls = row == null ? null : Controls(row);
        if (controls != null && _slot >= 0 && _slot < controls.Length && controls[_slot] != null)
            EventSystem.current?.SetSelectedGameObject(controls[_slot]!.gameObject);
    }

    private static Selectable?[] Controls(SoundRowUI row) =>
        new Selectable?[] { row.playButton, row.nameField, row.trimButton, row.editButton, row.imageButton, row.removeButton };

    private static int IndexOf(ThemeCreatorScreen screen, DraftSound sound)
    {
        var sounds = screen.draft?.Sounds;
        for (var index = 0; sounds != null && index < sounds.Count; index++)
            if (UiQueries.IsSameObject(sounds[index], sound))
                return index;
        return -1;
    }

    // The canvas is Screen Space Overlay (no camera), where world positions are already screen pixels.
    private static Vector2 ScreenCenter(Component row)
    {
        var rect = row.GetComponent<RectTransform>();
        var canvas = row.GetComponentInParent<Canvas>();
        var camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        return RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
    }
}
