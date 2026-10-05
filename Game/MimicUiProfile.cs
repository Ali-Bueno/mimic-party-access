using Il2CppInterop.Runtime;
using Mimick.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AccessKit;
using AccessKit.Text;
using AccessKit.Ui;

namespace MimicPartyAccess.Game;

/// <summary>
/// Mimic Party conventions: base screens derive from <c>Mimick.UI.UIScreen</c>, overlay panels expose their
/// heading as a <c>titleLabel</c> field, the settings panel captures raw keys while rebinding push-to-talk, and a
/// few buttons act as switches whose state is only an icon or a tint (room visibility, team search, friends drawer,
/// pack votes, invite picks, and the equipped/locked marks of character, animation and aura cells).
/// </summary>
public sealed class MimicUiProfile : DefaultUiProfile
{
    private const string TitleField = "titleLabel";

    // Character, animation and aura cells (SkinCellUI, RevealAnimationCellUI, AuraCellUI) share these image fields.
    private const string EquippedMarkField = "check";
    private const string LockMarkField = "padlock";

    public override string? GetBaseScreenKey()
    {
        foreach (var obj in UnityEngine.Object.FindObjectsByType(Il2CppType.Of<UIScreen>(), FindObjectsSortMode.None))
        {
            var screen = obj.TryCast<UIScreen>();
            if (screen != null && screen.isActiveAndEnabled)
                return screen.GetIl2CppType().Name;
        }
        return base.GetBaseScreenKey();
    }

    public override string? GetOverlayTitle(MonoBehaviour? owner)
    {
        var label = Il2CppFields.Read(owner, TitleField)?.TryCast<TMP_Text>();
        return label == null || UiQueries.GetVisibleAlpha(label) <= 0f ? null : UiQueries.ReadText(label);
    }

    /// <summary>Per-screen readers for controls whose name or state is only graphical (season pass tiers, shop, creator).</summary>
    public static readonly List<Func<Selectable, string?>> NameReaders = new();
    public static readonly List<Func<Selectable, string?>> StateReaders = new();

    public override string? GetWidgetName(Selectable selectable) => FirstAnswer(NameReaders, selectable);

    private static string? FirstAnswer(List<Func<Selectable, string?>> readers, Selectable selectable)
    {
        foreach (var reader in readers)
            if (reader(selectable) is string answer && answer.Length > 0)
                return answer;
        return null;
    }

    public override string? GetWidgetState(Selectable selectable)
    {
        if (FirstAnswer(StateReaders, selectable) is string state)
            return state;

        var visibility = selectable.GetComponent<VisibilityToggle>();
        if (visibility != null)
            return Strings.Get(visibility.IsPublic ? "state.room_public" : "state.room_private");

        var teamSearch = selectable.GetComponent<SearchTeamToggle>();
        if (teamSearch != null && teamSearch.stateImage != null && teamSearch.onSprite != null)
            return Strings.Get(UiQueries.IsSameObject(teamSearch.stateImage.sprite, teamSearch.onSprite) ? "ui.on" : "ui.off");

        var friends = selectable.GetComponentInParent<FriendsPanel>();
        if (friends != null && UiQueries.IsSameObject(friends.handleButton, selectable))
            return Strings.Get(friends.IsOpen ? "state.open" : "state.closed");

        if (CosmeticState(selectable) is string cosmetic)
            return cosmetic;

        // Friends picked in the invite list ("free character") are shown only by the row's picked tint.
        var inviteRow = selectable.GetComponentInParent<FriendRowUI>();
        if (inviteRow != null && UiQueries.IsSameObject(inviteRow.button, selectable))
            return Strings.Get(SameColor(inviteRow.background?.color, inviteRow.pickedTint) ? "ui.selected" : "ui.unselected");

        var voteRow = selectable.GetComponentInParent<ThemeVoteRow>();
        if (voteRow != null && UiQueries.IsSameObject(voteRow.likeButton, selectable))
            return VoteState(selectable, voteRow.likeOn);
        if (voteRow != null && UiQueries.IsSameObject(voteRow.reportButton, selectable))
            return VoteState(selectable, voteRow.reportOn);

        return null;
    }

    private static string? CosmeticState(Selectable selectable)
    {
        var cell = FindCell(selectable.transform);
        if (cell == null)
            return null;

        var equipped = IsShown(Il2CppFields.Read(cell, EquippedMarkField)?.TryCast<Image>());
        var locked = IsShown(Il2CppFields.Read(cell, LockMarkField)?.TryCast<Image>());
        return Strings.Get(equipped ? "state.equipped" : locked ? "state.locked" : "state.available");
    }

    // The cell script sits on the button or on its parent and holds both marks.
    private static MonoBehaviour? FindCell(Transform button)
    {
        foreach (var transform in new[] { button, button.parent })
        {
            if (transform == null)
                continue;
            foreach (var behaviour in transform.GetComponents<MonoBehaviour>())
                if (UiQueries.IsGameComponent(behaviour) && Il2CppFields.Read(behaviour, LockMarkField) != null)
                    return behaviour;
        }
        return null;
    }

    private static bool IsShown(Image? mark) =>
        mark != null && mark.gameObject.activeInHierarchy && UiQueries.GetVisibleAlpha(mark) > 0f;

    // ThemeVoteRow.Tint paints the chosen vote's button with its "on" colour.
    private static string VoteState(Selectable button, Color onColor)
    {
        var tinted = SameColor(button.targetGraphic?.color, onColor) || SameColor(button.colors.normalColor, onColor);
        return Strings.Get(tinted ? "ui.selected" : "ui.unselected");
    }

    private static bool SameColor(Color? actual, Color expected)
    {
        // One 8-bit colour step: UI colours round-trip through 8-bit values.
        const float tolerance = 1f / 255f;
        return actual is Color color && Math.Abs(color.r - expected.r) <= tolerance && Math.Abs(color.g - expected.g) <= tolerance &&
               Math.Abs(color.b - expected.b) <= tolerance;
    }

    public override bool IsCapturingInput(MonoBehaviour? overlayOwner) =>
        overlayOwner?.TryCast<PauseMenu>()?.capturingKey == true;
}
