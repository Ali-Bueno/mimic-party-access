namespace AccessKit.Ui;

/// <summary>
/// Decides which control a loose static text belongs to when labels and controls are siblings in one list:
/// the nearest control; on a near-tie, the control the text precedes (labels come before their control,
/// hints after it).
/// </summary>
public static class LabelAssignment
{
    // Rows in a list are spaced evenly, so a label sits as far from the control above as from the one below.
    // Gaps differing by less than half a text line are treated as equal.
    private const float TieFractionOfTextHeight = 0.5f;

    public static int NearestControl(ScreenRect text, IReadOnlyList<ScreenRect> controls)
    {
        var best = -1;
        var bestGap = float.MaxValue;
        for (var index = 0; index < controls.Count; index++)
        {
            var gap = Gap(text, controls[index]);
            var tie = best >= 0 && Math.Abs(gap - bestGap) <= text.Height * TieFractionOfTextHeight;
            var better = tie
                ? IsBefore(text, controls[index]) && !IsBefore(text, controls[best])
                : gap < bestGap;
            if (!better)
                continue;
            best = index;
            bestGap = Math.Min(gap, bestGap);
        }
        return best;
    }

    /// <summary>True when the text is read before the control: above it, or left of it on the same row.</summary>
    public static bool IsBefore(ScreenRect text, ScreenRect control) =>
        text.YMin >= control.YMax || (text.CenterX < control.XMin && text.CenterY > control.YMin && text.CenterY < control.YMax);

    public static float Gap(ScreenRect first, ScreenRect second)
    {
        var dx = Math.Max(0f, Math.Max(first.XMin, second.XMin) - Math.Min(first.XMax, second.XMax));
        var dy = Math.Max(0f, Math.Max(first.YMin, second.YMin) - Math.Min(first.YMax, second.YMax));
        return MathF.Sqrt(dx * dx + dy * dy);
    }
}
