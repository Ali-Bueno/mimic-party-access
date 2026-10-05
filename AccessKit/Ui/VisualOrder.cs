namespace AccessKit.Ui;

/// <summary>
/// Orders controls the way a sighted player reads the screen. Controls aligned in a column (a stack of buttons)
/// stay together; stacks whose rows line up form a grid read row by row; blocks are read top to bottom, then left
/// to right. Members of a scroll view are ordered inside it and placed where its viewport is.
/// Algorithm notes: reference/engines/bepinex/ugui-menu-pipeline.md.
/// </summary>
public static class VisualOrder
{
    /// <param name="rects">Screen rect of each control.</param>
    /// <param name="groups">Scroll-view id per control, or null when the control is not inside a scroll view.</param>
    /// <param name="groupViewports">Viewport rect of each scroll-view id.</param>
    public static List<int> Order(IReadOnlyList<ScreenRect> rects, IReadOnlyList<int?> groups, IReadOnlyDictionary<int, ScreenRect> groupViewports)
    {
        var outerRects = new List<ScreenRect>();
        var outerMembers = new List<List<int>>();
        foreach (var group in Enumerable.Range(0, rects.Count).GroupBy(index => groups[index]))
        {
            var members = group.ToList();
            if (group.Key is int id && groupViewports.TryGetValue(id, out var viewport))
            {
                var ordered = OrderRects(members.Select(index => rects[index]).ToList());
                outerRects.Add(viewport);
                outerMembers.Add(ordered.Select(position => members[position]).ToList());
                continue;
            }

            foreach (var index in members)
            {
                outerRects.Add(rects[index]);
                outerMembers.Add(new List<int> { index });
            }
        }

        return OrderRects(outerRects).SelectMany(position => outerMembers[position]).ToList();
    }

    /// <summary>Returns the indices of <paramref name="rects"/> in reading order.</summary>
    public static List<int> OrderRects(IReadOnlyList<ScreenRect> rects)
    {
        if (rects.Count <= 1)
            return Enumerable.Range(0, rects.Count).ToList();

        var blocks = MergeGrids(BuildStacks(rects), rects);
        var referenceHeight = Median(rects.Select(rect => rect.Height));
        var orderedBlocks = blocks
            .Select(block => (Members: RowMajor(block, rects), Bounds: block.Select(index => rects[index]).Aggregate(ScreenRect.Union)))
            .OrderByDescending(block => block.Bounds.YMax)
            .ToList();

        var result = new List<int>(rects.Count);
        foreach (var band in Bands(orderedBlocks, block => block.Bounds.YMax, referenceHeight))
            foreach (var block in band.OrderBy(block => block.Bounds.XMin))
                result.AddRange(block.Members);
        return result;
    }

    // Controls belong to one stack when they overlap horizontally by at least half the narrower width and the
    // vertical gap between them is smaller than the taller one (consecutive entries of the same column).
    private static List<List<int>> BuildStacks(IReadOnlyList<ScreenRect> rects)
    {
        var parent = Enumerable.Range(0, rects.Count).ToArray();
        int Find(int node) => parent[node] == node ? node : parent[node] = Find(parent[node]);

        for (var first = 0; first < rects.Count; first++)
            for (var second = first + 1; second < rects.Count; second++)
                if (IsStacked(rects[first], rects[second]))
                    parent[Find(first)] = Find(second);

        return Enumerable.Range(0, rects.Count).GroupBy(Find).Select(group => group.ToList()).ToList();
    }

    private static bool IsStacked(ScreenRect first, ScreenRect second)
    {
        var overlap = Math.Min(first.XMax, second.XMax) - Math.Max(first.XMin, second.XMin);
        var narrower = Math.Min(first.Width, second.Width);
        var gap = Math.Max(first.YMin, second.YMin) - Math.Min(first.YMax, second.YMax);
        return overlap >= narrower / 2f && gap < Math.Max(first.Height, second.Height);
    }

    // Two stacks whose entries sit on the same rows are columns of one grid; reading it row by row is natural.
    private static List<List<int>> MergeGrids(List<List<int>> stacks, IReadOnlyList<ScreenRect> rects)
    {
        var merged = true;
        while (merged)
        {
            merged = false;
            for (var first = 0; first < stacks.Count && !merged; first++)
                for (var second = first + 1; second < stacks.Count && !merged; second++)
                {
                    if (!RowsAlign(stacks[first], stacks[second], rects))
                        continue;
                    stacks[first].AddRange(stacks[second]);
                    stacks.RemoveAt(second);
                    merged = true;
                }
        }
        return stacks;
    }

    private static bool RowsAlign(List<int> first, List<int> second, IReadOnlyList<ScreenRect> rects)
    {
        if (first.Count < 2 || second.Count < 2)
            return false;
        var cell = rects[first[0]];
        if (!first.Concat(second).All(index => SameSize(rects[index], cell)))
            return false;

        var (smaller, larger) = first.Count <= second.Count ? (first, second) : (second, first);
        var aligned = smaller.Count(index => larger.Any(other => SameRow(rects[index], rects[other])));
        return aligned * 2 >= smaller.Count;
    }

    // Grid cells come from one prefab, so they share a size; the tolerance absorbs hover/scale animations.
    private const float GridCellSizeTolerance = 0.25f;

    private static bool SameSize(ScreenRect first, ScreenRect second) =>
        Math.Abs(first.Width - second.Width) <= GridCellSizeTolerance * Math.Min(first.Width, second.Width) &&
        Math.Abs(first.Height - second.Height) <= GridCellSizeTolerance * Math.Min(first.Height, second.Height);

    private static bool SameRow(ScreenRect first, ScreenRect second) =>
        Math.Abs(first.CenterY - second.CenterY) < Math.Min(first.Height, second.Height) / 2f;

    private static List<int> RowMajor(List<int> members, IReadOnlyList<ScreenRect> rects)
    {
        var byHeight = members.OrderByDescending(index => rects[index].CenterY).ToList();
        var rows = new List<List<int>>();
        foreach (var index in byHeight)
        {
            var row = rows.LastOrDefault();
            if (row != null && SameRow(rects[row[0]], rects[index]))
                row.Add(index);
            else
                rows.Add(new List<int> { index });
        }
        return rows.SelectMany(row => row.OrderBy(index => rects[index].XMin)).ToList();
    }

    private static IEnumerable<List<T>> Bands<T>(IEnumerable<T> sortedByTop, Func<T, float> top, float tolerance)
    {
        List<T>? band = null;
        var bandTop = 0f;
        foreach (var item in sortedByTop)
        {
            if (band != null && bandTop - top(item) < tolerance)
            {
                band.Add(item);
                continue;
            }
            if (band != null)
                yield return band;
            band = new List<T> { item };
            bandTop = top(item);
        }
        if (band != null)
            yield return band;
    }

    private static float Median(IEnumerable<float> values)
    {
        var sorted = values.OrderBy(value => value).ToList();
        return sorted.Count == 0 ? 0f : sorted[sorted.Count / 2];
    }
}
