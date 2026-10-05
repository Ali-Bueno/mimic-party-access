using AccessKit.Text;
using Xunit;

namespace MimicPartyAccess.Tests;

public class TextCleanerTests
{
    [Theory]
    [InlineData("<color=#FFF>JUGAR</color>", "JUGAR")]
    [InlineData("  Buscar\u00A0equipo \n ", "Buscar equipo")]
    [InlineData("Sensibilidad <b>50</b> %", "Sensibilidad 50 %")]
    [InlineData("Amigos <", "Amigos")]
    [InlineData("« Back", "Back")]
    [InlineData("a < b", "a b")]
    [InlineData("<3 x", "<3 x")]
    [InlineData(null, "")]
    public void CleanStripsRichTextAndWhitespace(string? input, string expected) =>
        Assert.Equal(expected, TextCleaner.Clean(input));

    [Theory]
    [InlineData("Musica 10 %", "10 %")]
    [InlineData("Sensibilidad del microfono 100 %", "100 %")]
    [InlineData("Volume 0.5", "0.5")]
    [InlineData("Idioma", null)]
    public void TrailingQuantityExtractsTheShownValue(string input, string? expected) =>
        Assert.Equal(expected, TextCleaner.TrailingQuantity(input));

    [Theory]
    [InlineData("SettingsButton", "Settings button")]
    [InlineData("pause_menu", "Pause menu")]
    public void HumanizeSplitsIdentifiers(string input, string expected) =>
        Assert.Equal(expected, TextCleaner.Humanize(input));
}
