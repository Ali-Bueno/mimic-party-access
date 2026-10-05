using AccessKit.Speech;
using AccessKit.Text;
using Mimick.Networking;
using Mimick.UI;

namespace MimicPartyAccess.Game.Creator;

/// <summary>Speaks the pack creator's status changes: toasts, recording, saving, publishing and trim notices.</summary>
internal static class CreatorAnnouncer
{
    // The same text reaching us through two game paths (toast plus result) within this window is spoken once.
    private static readonly TimeSpan RepeatWindow = TimeSpan.FromSeconds(1);

    private static string _lastText = "";
    private static DateTime _lastTime;
    private static string _lastRecordShape = "";

    public static void OnToast(string? message) => Speak(TextCleaner.Clean(message), interrupt: false);

    public static void OnTrimNotice(string? message) => Speak(TextCleaner.Clean(message), interrupt: false);

    public static void OnRecordingStarted(ThemeCreatorScreen screen)
    {
        if (screen.recording)
            Speak(Strings.Get("creator.record.started"), interrupt: true);
    }

    // The label may carry a countdown; only a change of its words is news.
    public static void OnRecordLabel(ThemeCreatorScreen screen, string? label)
    {
        var text = TextCleaner.Clean(label);
        var shape = new string(text.Where(character => !char.IsDigit(character)).ToArray());
        if (shape == _lastRecordShape)
            return;
        _lastRecordShape = shape;
        if (!screen.recording && text.Length > 0)
            Speak(Strings.Get("creator.record.label", text), interrupt: false);
    }

    public static void OnUploading(bool uploading)
    {
        if (uploading)
            Speak(Strings.Get("creator.upload.started"), interrupt: true);
    }

    public static void OnPublished(WorkshopPublishResult result)
    {
        var verdict = Strings.Get(result.Success ? "creator.publish.success" : "creator.publish.failed");
        if (result.NeedsAgreement)
            verdict += " " + Strings.Get("creator.publish.needs_agreement");
        var message = TextCleaner.Clean(result.Message);
        var spoken = ScreenReader.LastSpoken ?? "";
        if (message.Length > 0 && !spoken.Contains(message, StringComparison.OrdinalIgnoreCase))
            verdict += " " + message;
        Speak(verdict, interrupt: true);
    }

    public static void OnSaved(bool reveal, string? result)
    {
        if (reveal)
            Speak(Strings.Get(string.IsNullOrEmpty(result) ? "creator.save.failed" : "creator.save.done"), interrupt: true);
    }

    public static void Speak(string text, bool interrupt)
    {
        if (text.Length == 0)
            return;
        var now = DateTime.UtcNow;
        if (text == _lastText && now - _lastTime < RepeatWindow)
            return;
        _lastText = text;
        _lastTime = now;
        ScreenReader.Say(text, interrupt);
    }
}
