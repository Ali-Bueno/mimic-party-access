using UnityEngine;

namespace AccessKit.Ui;

/// <summary>Decides which of two UI objects uGUI renders on top (sorting layer, sorting order, then hierarchy order).</summary>
internal static class DrawOrder
{
    /// <summary>True when <paramref name="upper"/> is drawn after (on top of) <paramref name="lower"/>.</summary>
    public static bool IsDrawnAbove(Transform upper, Canvas upperCanvas, Transform lower, Canvas lowerCanvas)
    {
        if (upperCanvas.Pointer != lowerCanvas.Pointer)
        {
            var upperLayer = SortingLayer.GetLayerValueFromID(upperCanvas.sortingLayerID);
            var lowerLayer = SortingLayer.GetLayerValueFromID(lowerCanvas.sortingLayerID);
            if (upperLayer != lowerLayer)
                return upperLayer > lowerLayer;
            return upperCanvas.sortingOrder > lowerCanvas.sortingOrder;
        }

        return ComparePreorder(SiblingPath(upper, upperCanvas.transform), SiblingPath(lower, lowerCanvas.transform)) > 0;
    }

    private static List<int> SiblingPath(Transform transform, Transform root)
    {
        var path = new List<int>();
        for (var current = transform; current != null && current.Pointer != root.Pointer; current = current.parent)
            path.Add(current.GetSiblingIndex());
        path.Reverse();
        return path;
    }

    // Depth-first preorder: a parent renders before its children, earlier siblings before later ones.
    private static int ComparePreorder(List<int> first, List<int> second)
    {
        for (var i = 0; i < Math.Min(first.Count, second.Count); i++)
            if (first[i] != second[i])
                return first[i].CompareTo(second[i]);
        return first.Count.CompareTo(second.Count);
    }
}
