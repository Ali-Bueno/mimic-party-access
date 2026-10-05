namespace MimicPartyAccess.Game.Creator;

public static class CreatorPatches
{
    public static readonly Type[] PatchClasses =
    {
        typeof(CreatorIconButtonPatch),
        typeof(CreatorToastPatch),
        typeof(CreatorStartRecordingPatch),
        typeof(CreatorRecordLabelPatch),
        typeof(CreatorUploadingPatch),
        typeof(CreatorPublishedPatch),
        typeof(CreatorSavePatch),
        typeof(CreatorTrimNoticePatch),
    };
}
