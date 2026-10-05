namespace MimicPartyAccess.Game.Announcements;

/// <summary>Lists every announcement patch class so the plugin can register each one in isolation.</summary>
public static class GameAnnouncementPatches
{
    public static readonly Type[] PatchClasses =
    {
        typeof(GameScreenRoundStartedPatch),
        typeof(GameScreenPhaseChangedPatch),
        typeof(GameScreenScoredPatch),
        typeof(GameScreenRecordTurnPatch),
        typeof(GameScreenCountdownPatch),
        typeof(GameScreenWheelStoppedPatch),
        typeof(GameScreenWheelTargetingPatch),
        typeof(GameScreenWheelTargetingEndedPatch),
        typeof(GameScreenWheelEventPatch),
        typeof(PlayerIntroBannerShowPatch),
        typeof(GameScreenPerformanceReadyPatch),
        typeof(GameScreenPerformanceProgressPatch),
        typeof(LobbyThemeStartDownloadPatch),
        typeof(LobbyThemeStartDownloadAllPatch),
        typeof(LobbyThemeInstalledChangedPatch),
        typeof(LobbyStartPatch),
        typeof(LobbyRosterPatch),
        typeof(ResultsStartPatch),
        typeof(ResultsReplayWindowPatch),
        typeof(GameScreenPlaybackStartedPatch),
        typeof(GameScreenMicrophoneFailedPatch),
        typeof(GameScreenNetworkStatusPatch),
        typeof(GameScreenHostLostPatch),
        typeof(GameScreenTeamTurnPatch),
        typeof(GameScreenGameFinishedPatch),
        typeof(MalusLineupPlayPatch),
    };
}
