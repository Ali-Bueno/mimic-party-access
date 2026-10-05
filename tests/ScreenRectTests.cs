using AccessKit.Ui;
using Xunit;

namespace MimicPartyAccess.Tests;

public class ScreenRectTests
{
    private static readonly ScreenRect Screen = new(0, 0, 2560, 1440);

    [Theory]
    [InlineData(0, 0, 2560, 1440, true)]
    [InlineData(0.4f, 0.3f, 2559.6f, 1439.8f, true)]   // full-screen panel mid open-animation
    [InlineData(-10, -10, 2570, 1450, true)]
    [InlineData(0, 0, 2400, 1440, false)]                // side drawer
    [InlineData(707, 254, 1854, 786, false)]             // dialog window
    public void CoversScreenToleratesAnimationScale(float xMin, float yMin, float xMax, float yMax, bool expected) =>
        Assert.Equal(expected, new ScreenRect(xMin, yMin, xMax, yMax).CoversScreen(Screen));
}
