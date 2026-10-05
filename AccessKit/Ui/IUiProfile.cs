using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AccessKit.Ui;

/// <summary>
/// The thin, game-specific half of the menu pipeline. Everything here only adds context (names, titles);
/// it never decides whether a screen is read.
/// </summary>
public interface IUiProfile
{
    /// <summary>Identifies the base screen when no overlay is open; spoken via the strings key <c>layer.&lt;key&gt;</c>.</summary>
    string? GetBaseScreenKey();

    /// <summary>The title the game itself shows for an overlay panel, if it has one.</summary>
    string? GetOverlayTitle(MonoBehaviour? owner);

    /// <summary>Name of a game-specific control whose meaning is only graphical (an icon button); null keeps the generic name.</summary>
    string? GetWidgetName(Selectable selectable);

    /// <summary>State of a game-specific control that shows it only graphically (icon, colour), e.g. a button acting as a switch.</summary>
    string? GetWidgetState(Selectable selectable);

    /// <summary>True while the game reads raw keys (e.g. a key-rebinding prompt) and menu keys must pass through.</summary>
    bool IsCapturingInput(MonoBehaviour? overlayOwner);
}

/// <summary>Works for any uGUI game: the scene name identifies the base screen, overlay titles come from their texts.</summary>
public class DefaultUiProfile : IUiProfile
{
    public virtual string? GetBaseScreenKey() => SceneManager.GetActiveScene().name;
    public virtual string? GetOverlayTitle(MonoBehaviour? owner) => null;
    public virtual string? GetWidgetName(Selectable selectable) => null;
    public virtual string? GetWidgetState(Selectable selectable) => null;
    public virtual bool IsCapturingInput(MonoBehaviour? overlayOwner) => false;
}
