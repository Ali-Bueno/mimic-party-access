using AccessKit.Text;
using AccessKit.Ui;
using Mimick.UI;
using UnityEngine.UI;

namespace MimicPartyAccess.Game.Creator;

/// <summary>Names and states for the controls of a sound row (<c>SoundRowUI</c>) and for the import drop zones.</summary>
internal static class RowReader
{
    public static string? NameOf(Selectable selectable)
    {
        var row = selectable.GetComponentInParent<SoundRowUI>();
        if (row != null)
            return RowControlName(row, selectable);
        return DropZoneName(selectable);
    }

    public static string? StateOf(Selectable selectable)
    {
        var row = selectable.GetComponentInParent<SoundRowUI>();
        if (row == null)
            return null;
        if (Is(row.playButton, selectable))
        {
            var duration = UiQueries.ReadText(row.durationLabel);
            return duration.Length > 0 ? Strings.Get("creator.row.duration", duration) : null;
        }
        if (Is(row.imageButton, selectable))
            return Strings.Get(IsShown(row.thumbnailPlaceholder) ? "creator.row.no_image" : "creator.row.has_image");
        return null;
    }

    private static string? RowControlName(SoundRowUI row, Selectable control)
    {
        var label = Label(row);
        if (Is(row.playButton, control)) return Strings.Get("creator.row.play", label);
        if (Is(row.trimButton, control)) return Strings.Get("creator.row.trim", label);
        if (Is(row.editButton, control)) return Strings.Get("creator.row.rename", label);
        if (Is(row.imageButton, control)) return Strings.Get("creator.row.image", label);
        if (Is(row.removeButton, control)) return Strings.Get("creator.row.remove", label);
        if (Is(row.nameField, control)) return Strings.Get("creator.row.name_field", UiQueries.ReadText(row.indexLabel));
        return null;
    }

    private static string Label(SoundRowUI row)
    {
        var index = UiQueries.ReadText(row.indexLabel);
        var name = TextCleaner.Clean(row.nameField?.text);
        var label = Strings.Get("creator.row.label", index).Trim();
        return name.Length > 0 ? $"{label}, {name}" : label;
    }

    private static string? DropZoneName(Selectable control)
    {
        var zone = control.GetComponentInParent<DropZone>();
        if (zone == null)
            return null;
        var screen = CreatorLookup.FindActive<ThemeCreatorScreen>();
        if (screen == null)
            return null;
        var parts = UiQueries.IsSameObject(zone, screen.soundDropZone)
            ? new[] { UiQueries.ReadText(screen.soundDropTitle), UiQueries.ReadText(screen.soundDropSubtitle) }
            : UiQueries.IsSameObject(zone, screen.imageDropZone) ? new[] { UiQueries.ReadText(screen.imageDropTitle) } : [];
        var name = string.Join(". ", parts.Where(part => part.Length > 0));
        return name.Length > 0 ? name : null;
    }

    private static bool Is(UnityEngine.Object? expected, Selectable control) => UiQueries.IsSameObject(expected, control);

    private static bool IsShown(Image? image) => image != null && image.gameObject.activeInHierarchy;
}
