using AccessKit.Audio;
using Mimick.Audio;
using UnityEngine;

namespace MimicPartyAccess.Game;

/// <summary>
/// While the local player records, a soft tone follows the loudness of the reference sound at the current moment of
/// the take — the audio equivalent of the reference waveform sighted players see on the performance bar.
/// </summary>
public static class RecordingGuide
{
    // A low hum stays distinct from voices and from the game's own cue sounds.
    private const float ToneFrequencyHz = 220f;

    // Loudness is measured over windows this long: short enough to follow syllables, long enough not to buzz.
    private const float EnvelopeWindowSeconds = 0.05f;

    // The bar stops receiving progress when the take ends; silence the tone once updates stop for this long.
    private static readonly TimeSpan SilenceAfterLastUpdate = TimeSpan.FromMilliseconds(300);

    private static ToneCue? _tone;
    private static float[] _envelope = Array.Empty<float>();
    private static float _referenceSeconds, _leadInSeconds, _tailSeconds;
    private static DateTime _lastUpdate;

    public static void OnPerformanceReady(AudioSamples? reference, float leadIn, float tail)
    {
        if (!MimicConfig.RecordingGuide || reference == null || reference.IsEmpty)
            return;

        _leadInSeconds = leadIn;
        _tailSeconds = tail;
        _referenceSeconds = reference.Duration;
        _envelope = Envelope(reference);
    }

    /// <param name="progress">Position on the bar, 0–1, across lead-in, reference and tail.</param>
    public static void OnProgress(float progress)
    {
        if (!MimicConfig.RecordingGuide || _envelope.Length == 0)
            return;

        var seconds = progress * (_leadInSeconds + _referenceSeconds + _tailSeconds) - _leadInSeconds;
        var index = (int)(seconds / EnvelopeWindowSeconds);
        var level = index >= 0 && index < _envelope.Length ? _envelope[index] : 0f;
        _tone ??= new ToneCue("MimicPartyAccess.RecordingGuide", ToneFrequencyHz);
        _tone.SetVolume(level * MimicConfig.RecordingGuideVolume);
        _lastUpdate = DateTime.UtcNow;
    }

    public static void Tick()
    {
        if (_tone != null && DateTime.UtcNow - _lastUpdate > SilenceAfterLastUpdate)
            _tone.Stop();
    }

    // Root-mean-square per window, normalised to the loudest window.
    private static float[] Envelope(AudioSamples reference)
    {
        var data = reference.Data;
        var window = Math.Max(1, (int)(reference.SampleRate * EnvelopeWindowSeconds));
        var envelope = new float[(data.Length + window - 1) / window];
        var loudest = 0f;
        for (var w = 0; w < envelope.Length; w++)
        {
            var sum = 0f;
            var end = Math.Min(data.Length, (w + 1) * window);
            for (var i = w * window; i < end; i++)
                sum += data[i] * data[i];
            envelope[w] = Mathf.Sqrt(sum / Math.Max(1, end - w * window));
            loudest = Math.Max(loudest, envelope[w]);
        }

        if (loudest > 0f)
            for (var w = 0; w < envelope.Length; w++)
                envelope[w] /= loudest;
        return envelope;
    }
}
