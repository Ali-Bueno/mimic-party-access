using AccessKit.Speech;
using AccessKit.Text;
using AccessKit.Ui;
using Mimick.Data;
using Mimick.UI;

namespace MimicPartyAccess.Game.Announcements;

/// <summary>
/// End-of-round wheel: the slot it lands on (the game's own localized texts), its event lines, and target picking —
/// which the game does by clicking a character on the 3D stage, offered here as a choice list that calls the same
/// <c>RoundController.ChooseWheelTarget</c>.
/// </summary>
internal static class WheelAnnouncer
{
    public static void OnStopped(WheelSlot slot)
    {
        var title = TextCleaner.Clean(slot.LocalizedTitle);
        var description = TextCleaner.Clean(slot.LocalizedDescription);
        var result = string.Join(". ", new[] { title, description }.Where(part => part.Length > 0).Select(part => part.TrimEnd('.')));
        if (result.Length > 0)
            ScreenReader.Say(Strings.Get("game.wheel.result", result), interrupt: true);
    }

    public static void OnTargetingStarted(GameScreen screen, Il2CppSystem.Collections.Generic.IReadOnlyList<MimickPlayer>? allowed)
    {
        var controller = screen.roundController;
        var count = allowed?.TryCast<Il2CppSystem.Collections.Generic.ICollection<MimickPlayer>>()?.Count ?? 0;
        var choices = new List<(string Label, Action Choose)>();
        for (var index = 0; index < count && controller != null; index++)
        {
            var player = allowed![index];
            var name = TextCleaner.Clean(player?.DisplayName);
            if (player == null || name.Length == 0)
                continue;
            choices.Add((name, () =>
            {
                controller.ChooseWheelTarget(player);
                ScreenReader.Say(Strings.Get("game.wheel.target_chosen", name), interrupt: true);
            }));
        }

        if (choices.Count == 0)
            ScreenReader.Say(Strings.Get("game.wheel.pick_target"), interrupt: true);
        else
            ChoicePrompt.Show(Strings.Get("game.wheel.pick_target"), choices);
    }

    public static void OnTargetingEnded() => ChoicePrompt.Close();

    public static void OnEvent(string line) =>
        ScreenReader.Say(TextCleaner.Clean(line));
}
