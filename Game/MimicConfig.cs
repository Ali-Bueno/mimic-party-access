using BepInEx.Configuration;

namespace MimicPartyAccess.Game;

/// <summary>Mimic Party–specific switches, next to the shared ones in AccessKit.ModConfig.</summary>
public static class MimicConfig
{
    private static ConfigEntry<bool>? _recordingGuide;
    private static ConfigEntry<float>? _recordingGuideVolume;

    public static bool RecordingGuide => _recordingGuide?.Value ?? true;
    public static float RecordingGuideVolume => _recordingGuideVolume?.Value ?? 0.25f;

    public static void Bind(ConfigFile config)
    {
        _recordingGuide = config.Bind("Features", "RecordingGuide", true,
            "While you record, a soft tone follows the loudness of the reference sound (what sighted players see on the bar). Use headphones: speakers would leak it into your take.");
        _recordingGuideVolume = config.Bind("Features", "RecordingGuideVolume", 0.25f,
            new ConfigDescription("Loudness of the recording guide tone.", new AcceptableValueRange<float>(0f, 1f)));
    }
}
