namespace AccessKit.Ui;

/// <summary>An axis-aligned rectangle in screen pixels (Unity convention: origin bottom-left, y grows upwards).</summary>
public readonly record struct ScreenRect(float XMin, float YMin, float XMax, float YMax)
{
    public float Width => XMax - XMin;
    public float Height => YMax - YMin;
    public float CenterX => (XMin + XMax) / 2f;
    public float CenterY => (YMin + YMax) / 2f;
    public bool IsEmpty => Width <= 0f || Height <= 0f;

    public bool Contains(ScreenRect other) =>
        XMin <= other.XMin && YMin <= other.YMin && XMax >= other.XMax && YMax >= other.YMax;

    // Open/close animations scale panels slightly; a graphic within 1 % of every screen edge still covers it.
    private const float FullScreenMargin = 0.01f;

    /// <summary>True when this rect covers the whole screen (a modal backdrop or a full-screen panel).</summary>
    public bool CoversScreen(ScreenRect screen)
    {
        var marginX = screen.Width * FullScreenMargin;
        var marginY = screen.Height * FullScreenMargin;
        return XMin <= screen.XMin + marginX && YMin <= screen.YMin + marginY &&
               XMax >= screen.XMax - marginX && YMax >= screen.YMax - marginY;
    }

    public bool Intersects(ScreenRect other) =>
        XMin < other.XMax && other.XMin < XMax && YMin < other.YMax && other.YMin < YMax;

    public static ScreenRect Union(ScreenRect first, ScreenRect second) => new(
        Math.Min(first.XMin, second.XMin), Math.Min(first.YMin, second.YMin),
        Math.Max(first.XMax, second.XMax), Math.Max(first.YMax, second.YMax));

    public override string ToString() => $"[{XMin:0},{YMin:0} {Width:0}x{Height:0}]";
}
