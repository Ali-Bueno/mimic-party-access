using System.Reflection;
using System.Runtime.InteropServices;

namespace AccessKit.Speech;

/// <summary>The single speech sink: every announcement in the mod goes through here to PRISM.</summary>
public static class ScreenReader
{
    private static IntPtr _context;
    private static IntPtr _backend;
    private static bool _resolverInstalled;
    private static DateTime _nextAcquireAttempt;

    // Bounds how often a failing backend is re-acquired, so a missing screen reader does not cost a lookup per line.
    private static readonly TimeSpan ReacquireInterval = TimeSpan.FromSeconds(5);

    public static string? LastSpoken { get; private set; }
    public static bool LogSpeech { get; set; }
    public static bool IsAvailable => _backend != IntPtr.Zero;

    public static void Initialize(string pluginDirectory)
    {
        try
        {
            InstallResolver(pluginDirectory);
            var config = PrismNative.prism_config_init();
            _context = PrismNative.prism_init(ref config);
            if (_context == IntPtr.Zero)
            {
                ModLog.Error("PRISM initialization failed; speech is disabled.");
                return;
            }

            AcquireBackend();
        }
        catch (Exception exception) when (exception is DllNotFoundException or EntryPointNotFoundException or BadImageFormatException)
        {
            ModLog.Error($"Could not load {PrismNative.Library} next to the plugin: {exception.Message}");
        }
    }

    public static void Say(string? text, bool interrupt = false)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        LastSpoken = text;
        if (LogSpeech)
            ModLog.Info($"[speech{(interrupt ? "!" : "")}] {text}");
        if (_context == IntPtr.Zero)
            return;

        var result = Output(text, interrupt);
        if ((result is PrismNative.ErrorBackendNotAvailable or PrismNative.ErrorNotInitialized) && TryReacquire())
            Output(text, interrupt);
    }

    private static bool TryReacquire()
    {
        if (DateTime.UtcNow < _nextAcquireAttempt)
            return false;
        _nextAcquireAttempt = DateTime.UtcNow + ReacquireInterval;
        return AcquireBackend();
    }

    public static void Repeat()
    {
        if (LastSpoken != null)
            Say(LastSpoken, interrupt: true);
    }

    public static void Silence()
    {
        if (_backend != IntPtr.Zero)
            _ = PrismNative.prism_backend_stop(_backend);
    }

    public static void Shutdown()
    {
        ReleaseBackend();
        if (_context != IntPtr.Zero)
            PrismNative.prism_shutdown(_context);
        _context = IntPtr.Zero;
    }

    private static int Output(string text, bool interrupt)
    {
        return _backend == IntPtr.Zero
            ? PrismNative.ErrorBackendNotAvailable
            : PrismNative.prism_backend_output(_backend, text, interrupt);
    }

    // Re-acquiring lets a screen reader started after the game take over from the fallback backend.
    private static bool AcquireBackend()
    {
        ReleaseBackend();
        _backend = PrismNative.prism_registry_acquire_best(_context);
        if (_backend == IntPtr.Zero)
        {
            ModLog.Warning("PRISM found no usable speech backend.");
            return false;
        }

        var status = PrismNative.prism_backend_initialize(_backend);
        if (status != PrismNative.Ok && status != PrismNative.ErrorAlreadyInitialized)
        {
            ModLog.Warning($"PRISM backend failed to initialize (error {status}).");
            ReleaseBackend();
            return false;
        }

        ModLog.Info($"PRISM speech backend: {Marshal.PtrToStringUTF8(PrismNative.prism_backend_name(_backend))}");
        return true;
    }

    private static void ReleaseBackend()
    {
        if (_backend != IntPtr.Zero)
            PrismNative.prism_backend_free(_backend);
        _backend = IntPtr.Zero;
    }

    // prism.dll ships next to the plugin, which is not on the default native search path of the game process.
    private static void InstallResolver(string pluginDirectory)
    {
        if (_resolverInstalled)
            return;

        _resolverInstalled = true;
        var libraryPath = Path.Combine(pluginDirectory, PrismNative.Library);
        NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), (name, _, _) =>
            name == PrismNative.Library && File.Exists(libraryPath) ? NativeLibrary.Load(libraryPath) : IntPtr.Zero);
    }
}
