using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace AccessKit.Audio;

/// <summary>
/// A continuous sine tone played through Unity's own audio (no extra libraries), whose loudness the caller drives —
/// for audio cues that follow a value over time.
/// </summary>
public sealed class ToneCue
{
    private readonly AudioSource _source;

    public ToneCue(string name, float frequencyHz)
    {
        var host = new GameObject(name) { hideFlags = HideFlags.HideAndDontSave };
        UnityEngine.Object.DontDestroyOnLoad(host);
        _source = host.AddComponent<AudioSource>();
        _source.clip = CreateSineLoop(name, frequencyHz);
        _source.loop = true;
        _source.playOnAwake = false;
        _source.spatialBlend = 0f;
        _source.volume = 0f;
    }

    /// <summary>Sets the loudness (0–1); the tone starts on the first non-zero value.</summary>
    public void SetVolume(float volume)
    {
        _source.volume = Mathf.Clamp01(volume);
        if (volume > 0f && !_source.isPlaying)
            _source.Play();
    }

    public void Stop()
    {
        _source.volume = 0f;
        if (_source.isPlaying)
            _source.Stop();
    }

    // One second at the output rate holds a whole number of cycles for an integer frequency, so the loop is seamless.
    private static AudioClip CreateSineLoop(string name, float frequencyHz)
    {
        var sampleRate = AudioSettings.outputSampleRate;
        var samples = new Il2CppStructArray<float>(sampleRate);
        var cycles = Mathf.Round(frequencyHz);
        for (var i = 0; i < sampleRate; i++)
            samples[i] = Mathf.Sin(2f * Mathf.PI * cycles * i / sampleRate);

        var clip = AudioClip.Create(name, sampleRate, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
