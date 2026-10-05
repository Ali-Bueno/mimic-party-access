using AccessKit.Ui;
using Mimick.UI;
using TMPro;

namespace MimicPartyAccess.Game.Creator;

/// <summary>Keyboard replacement for dragging the trim panel's grips: nudges the selection and reads the game's own labels.</summary>
internal static class TrimKeys
{
    // A step small enough to trim precisely; the panel's own labels confirm the result after every nudge.
    private const float NudgeSeconds = 0.1f;

    public static SoundTrimPanel? ActivePanel() => CreatorLookup.FindActive<SoundTrimPanel>();

    public static bool TryNudge(int direction, bool adjustLength)
    {
        var panel = ActivePanel();
        if (panel == null || IsEditingText())
            return false;

        var delta = direction * NudgeSeconds;
        if (adjustLength)
            panel.Nudge(0f, delta);
        else
            panel.Nudge(delta, 0f);
        panel.Refresh();
        CreatorAnnouncer.Speak(Readout(panel), interrupt: true);
        return true;
    }

    public static string Readout(SoundTrimPanel panel) =>
        string.Join(", ", new[] { UiQueries.ReadText(panel.startLabel), UiQueries.ReadText(panel.lengthLabel) }.Where(text => text.Length > 0));

    // Ctrl+Left/Right already means previous/next word inside a focused text field.
    private static bool IsEditingText() =>
        CreatorLookup.Selected()?.GetComponent<TMP_InputField>() is TMP_InputField field && field.isFocused;
}
