using Il2CppInterop.Runtime;
using Mimick.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MimicPartyAccess.Game.Creator;

/// <summary>Finds the live creator screens and the control that currently has keyboard focus.</summary>
internal static class CreatorLookup
{
    public static T? FindActive<T>() where T : MonoBehaviour
    {
        foreach (var candidate in UnityEngine.Object.FindObjectsByType(Il2CppType.Of<T>(), FindObjectsSortMode.None))
        {
            var found = candidate.TryCast<T>();
            if (found != null && found.isActiveAndEnabled)
                return found;
        }
        return null;
    }

    public static GameObject? Selected() => EventSystem.current?.currentSelectedGameObject;

    /// <summary>The visible sound rows in on-screen order.</summary>
    public static List<SoundRowUI> Rows(ThemeCreatorScreen screen)
    {
        var rows = new List<SoundRowUI>();
        var list = screen.soundList;
        for (var index = 0; list != null && index < list.childCount; index++)
        {
            var row = list.GetChild(index).GetComponent<SoundRowUI>();
            if (row != null && row.gameObject.activeInHierarchy)
                rows.Add(row);
        }
        return rows;
    }
}
