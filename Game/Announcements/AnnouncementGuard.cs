using AccessKit;

namespace MimicPartyAccess.Game.Announcements;

/// <summary>Runs announcement logic so a failure never propagates into the game.</summary>
internal static class AnnouncementGuard
{
    public static void Run(string name, Action action)
    {
        if (!ModConfig.GameAnnouncements)
            return;

        try
        {
            action();
        }
        catch (Exception exception)
        {
            ModLog.WarningOnce($"announce.{name}", $"Announcement '{name}' failed: {exception.Message}");
        }
    }
}
