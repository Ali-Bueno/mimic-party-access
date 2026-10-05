using AccessKit.Ui;
using Xunit;

namespace MimicPartyAccess.Tests;

public class VisualOrderTests
{
    private static ScreenRect R(float x, float y, float width, float height) => new(x, y, x + width, y + height);

    private static List<string> Order(IReadOnlyList<(string Name, ScreenRect Rect)> items) =>
        VisualOrder.OrderRects(items.Select(item => item.Rect).ToList()).Select(index => items[index].Name).ToList();

    // Rects captured from Mimic Party's main menu (2560x1440, ui-dump.txt).
    private static readonly (string Name, ScreenRect Rect)[] MainMenu =
    {
        ("CreateRoom", R(1338, 639, 613, 149)), ("SearchTeam", R(284, 223, 187, 71)),
        ("Objectives", R(35, 344, 293, 85)), ("Discord", R(35, 214, 293, 85)),
        ("Invite", R(37, 33, 291, 85)), ("Quit", R(1338, 71, 613, 128)),
        ("Card", R(1058, 911, 313, 360)), ("ThemeCreator", R(1338, 227, 613, 128)),
        ("Visibility", R(1886, 673, 333, 81)), ("Settings", R(2440, 1054, 138, 74)),
        ("Play", R(1337, 797, 613, 166)), ("Auras", R(2143, 163, 402, 137)),
        ("Animations", R(2143, 447, 402, 137)), ("Skins", R(2143, 307, 402, 137)),
        ("Info", R(35, 126, 293, 85)), ("MimiBucks", R(2169, 1178, 423, 123)),
        ("BattlePass", R(1338, 479, 613, 149)), ("Handle", R(2429, 656, 131, 128)),
    };

    [Fact]
    public void ColumnsOfButtonsAreReadContiguouslyTopToBottom()
    {
        var order = Order(MainMenu);

        AssertContiguous(order, "Play", "CreateRoom", "BattlePass", "ThemeCreator", "Quit");
        AssertContiguous(order, "Objectives", "Discord", "Info", "Invite");
        AssertContiguous(order, "Animations", "Skins", "Auras");
        Assert.Equal(MainMenu.Length, order.Distinct().Count());
    }

    [Fact]
    public void ScrollViewIsReadWhereItsViewportIsNotWhereItsContentOverflows()
    {
        // Settings-like panel: tabs on top, a scroll view whose content runs below the screen, a fixed button under it.
        var rects = new List<ScreenRect>
        {
            R(864, 969, 404, 133), R(1292, 969, 404, 133),          // tabs
            R(864, 801, 832, 75), R(864, 351, 832, 43), R(864, 183, 832, 75), R(864, -84, 832, 75), // scrolled items
            R(864, 156, 832, 85),                                    // resume button below the viewport
        };
        var groups = new List<int?> { null, null, 0, 0, 0, 0, null };
        var viewports = new Dictionary<int, ScreenRect> { [0] = R(864, 260, 832, 680) };

        var order = VisualOrder.Order(rects, groups, viewports);

        Assert.Equal(new[] { 0, 1, 2, 3, 4, 5, 6 }, order);
    }

    [Fact]
    public void GridIsReadRowByRow()
    {
        var cells = new List<(string, ScreenRect)>();
        for (var row = 0; row < 3; row++)
            for (var column = 0; column < 3; column++)
                cells.Add(($"r{row}c{column}", R(100 + column * 220, 900 - row * 220, 200, 200)));

        var order = Order(cells);

        Assert.Equal(cells.Select(cell => cell.Item1), order);
    }

    [Fact]
    public void ButtonsSideBySideAreReadLeftToRight()
    {
        var order = Order(new[] { ("Confirm", R(1292, 560, 431, 133)), ("Cancel", R(837, 560, 428, 133)) });

        Assert.Equal(new[] { "Cancel", "Confirm" }, order);
    }

    private static void AssertContiguous(List<string> order, params string[] expected)
    {
        var start = order.IndexOf(expected[0]);
        Assert.True(start >= 0, $"{expected[0]} missing");
        Assert.Equal(expected, order.Skip(start).Take(expected.Length));
    }
}
