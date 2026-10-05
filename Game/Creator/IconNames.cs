using AccessKit;
using AccessKit.Text;
using Mimick.UI;
using UnityEngine.UI;

namespace MimicPartyAccess.Game.Creator;

/// <summary>Remembers the name the game gives each creator icon button when it builds it, and speaks it as the control's name.</summary>
internal static class IconNames
{
    // The creator holds a few dozen buttons; far above that means rebuilt rows left destroyed entries behind.
    private const int PruneThreshold = 256;

    private static readonly Dictionary<int, (Button Button, string Name)> Known = new();

    public static void Remember(Button? button, string? name, UiIcons.Kind kind)
    {
        try
        {
            if (button == null)
                return;
            if (Known.Count >= PruneThreshold)
                Prune();
            Known[button.GetInstanceID()] = (button, Resolve(name, kind));
        }
        catch (Exception exception)
        {
            ModLog.WarningOnce("creator.icon", $"Icon button name not recorded: {exception.Message}");
        }
    }

    public static string? NameOf(Selectable selectable) =>
        Known.TryGetValue(selectable.GetInstanceID(), out var entry) ? entry.Name : null;

    // A multi-word name is a label the game wrote for people; a single word is an identifier, so our own wording wins.
    private static string Resolve(string? name, UiIcons.Kind kind)
    {
        var cleaned = TextCleaner.Clean(name);
        if (cleaned.Contains(' '))
            return cleaned;
        if (Strings.TryGet("creator.icon." + kind.ToString().ToLowerInvariant(), out var own))
            return own;
        return cleaned.Length > 0 ? TextCleaner.Humanize(cleaned) : kind.ToString();
    }

    private static void Prune()
    {
        foreach (var id in Known.Where(pair => pair.Value.Button == null).Select(pair => pair.Key).ToList())
            Known.Remove(id);
    }
}
