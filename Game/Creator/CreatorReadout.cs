using AccessKit.Text;
using AccessKit.Ui;
using Mimick.UI;

namespace MimicPartyAccess.Game.Creator;

/// <summary>On demand: the trim state, or the creator's progress, sound counts and whether saving/publishing is available.</summary>
internal static class CreatorReadout
{
    public static bool Speak()
    {
        var trim = TrimKeys.ActivePanel();
        var text = trim != null ? TrimKeys.Readout(trim) : ScreenReadout();
        if (text.Length == 0)
            return false;
        CreatorAnnouncer.Speak(text, interrupt: true);
        return true;
    }

    private static string ScreenReadout()
    {
        var screen = CreatorLookup.FindActive<ThemeCreatorScreen>();
        if (screen == null)
            return "";

        var progress = string.Join(" ", new[] { UiQueries.ReadText(screen.progressLabel), UiQueries.ReadText(screen.progressCount) }
            .Where(text => text.Length > 0));
        var parts = new List<string> { progress, UiQueries.ReadText(screen.soundCountLabel), UiQueries.ReadText(screen.minSoundsLabel) };
        if (screen.saveButton != null && !screen.saveButton.interactable)
            parts.Add(Strings.Get("creator.state.save_locked"));
        if (screen.publishButton != null && !screen.publishButton.interactable)
            parts.Add(Strings.Get("creator.state.publish_locked"));
        return string.Join(". ", parts.Where(part => part.Length > 0));
    }
}
