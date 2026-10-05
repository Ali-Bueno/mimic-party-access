using UnityEngine;

namespace AccessKit;

/// <summary>Injected MonoBehaviour that gives the plugin a per-frame tick (a BasePlugin has no Unity lifecycle).</summary>
public sealed class ModDriver : MonoBehaviour
{
    private static readonly List<(string Name, Action Tick)> Ticks = new();

    public ModDriver(IntPtr pointer) : base(pointer) { }

    /// <summary>Registers a per-frame callback; a throwing callback is logged once and keeps running for others.</summary>
    public static void Register(string name, Action tick) => Ticks.Add((name, tick));

    private void Update()
    {
        foreach (var (name, tick) in Ticks)
        {
            try
            {
                tick();
            }
            catch (Exception exception)
            {
                ModLog.WarningOnce($"tick:{name}:{exception.GetType().Name}", $"{name} tick failed: {exception}");
            }
        }
    }
}
