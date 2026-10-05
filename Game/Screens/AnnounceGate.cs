namespace MimicPartyAccess.Game.Screens;

/// <summary>Remembers the last text spoken per topic so a refresh that changes nothing stays silent.</summary>
internal static class AnnounceGate
{
    private static readonly Dictionary<string, string> Last = new();

    public static bool Changed(string topic, string text)
    {
        if (text.Length == 0 || Last.TryGetValue(topic, out var previous) && previous == text)
            return false;
        Last[topic] = text;
        return true;
    }

    public static void Reset(string topic) => Last.Remove(topic);
}
