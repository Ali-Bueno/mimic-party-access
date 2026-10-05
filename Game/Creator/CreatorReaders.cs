using AccessKit;

namespace MimicPartyAccess.Game.Creator;

/// <summary>Registers the creator's name and state readers with the Mimic Party UI profile.</summary>
public static class CreatorReaders
{
    public static void Register()
    {
        MimicUiProfile.NameReaders.Add(Safe(selectable => RowReader.NameOf(selectable) ?? IconNames.NameOf(selectable)));
        MimicUiProfile.StateReaders.Add(Safe(RowReader.StateOf));
    }

    // Readers run for every control on every screen: never let one throw.
    private static Func<UnityEngine.UI.Selectable, string?> Safe(Func<UnityEngine.UI.Selectable, string?> reader) =>
        selectable =>
        {
            try
            {
                return reader(selectable);
            }
            catch (Exception exception)
            {
                ModLog.WarningOnce("creator.reader", $"Creator reader failed: {exception.Message}");
                return null;
            }
        };
}
