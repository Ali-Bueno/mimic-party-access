using AccessKit.Ui;
using Xunit;

namespace MimicPartyAccess.Tests;

public class LabelAssignmentTests
{
    private static ScreenRect R(float x, float y, float width, float height) => new(x, y, x + width, y + height);

    // Mimic Party's settings list (ui-dump.txt): labels and controls are siblings of one scroll content.
    private static readonly (string Name, ScreenRect Rect)[] Controls =
    {
        ("Language", R(864, 801, 832, 75)), ("Microphone", R(864, 633, 832, 75)),
        ("Sensitivity", R(864, 351, 832, 43)), ("PushToTalk", R(864, 183, 832, 75)),
        ("PushToTalkKey", R(864, 84, 832, 75)), ("Region", R(864, -84, 832, 75)),
        ("Display", R(864, -316, 832, 75)), ("Voices", R(864, -452, 832, 43)),
        ("Music", R(864, -588, 832, 43)),
    };

    [Theory]
    [InlineData("Idioma", 864, 900, 832, 45, "Language")]
    [InlineData("Microfono", 864, 732, 832, 45, "Microphone")]
    [InlineData("Eleccion automatica.", 864, 569, 832, 40, "Microphone")]
    [InlineData("Sensibilidad 100 %", 864, 417, 832, 45, "Sensitivity")]
    [InlineData("Pulsar para hablar", 864, 281, 832, 45, "PushToTalk")]
    [InlineData("Region del servidor", 864, 15, 832, 45, "Region")]
    [InlineData("Debe coincidir...", 864, -148, 832, 40, "Region")]
    [InlineData("Pantalla", 864, -217, 832, 45, "Display")]
    [InlineData("Voces 100 %", 864, -385, 832, 45, "Voices")]
    [InlineData("Musica 10 %", 864, -521, 832, 45, "Music")]
    public void EachTextGoesToItsControl(string text, float x, float y, float width, float height, string expected)
    {
        var index = LabelAssignment.NearestControl(R(x, y, width, height), Controls.Select(control => control.Rect).ToList());

        Assert.True(index >= 0, text);
        Assert.Equal(expected, Controls[index].Name);
    }

    [Fact]
    public void LabelLeftOfControlOnTheSameRowIsBefore() =>
        Assert.True(LabelAssignment.IsBefore(R(100, 500, 200, 40), R(400, 490, 300, 60)));

    [Fact]
    public void HintBelowControlIsAfter() =>
        Assert.False(LabelAssignment.IsBefore(R(864, 569, 832, 40), R(864, 633, 832, 75)));
}
