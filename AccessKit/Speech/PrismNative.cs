using System.Runtime.InteropServices;

namespace AccessKit.Speech;

/// <summary>P/Invoke surface of the PRISM C API (signatures from include/prism.h, PRISM v0.18.3).</summary>
internal static class PrismNative
{
    public const string Library = "prism.dll";

    public const int Ok = 0;
    public const int ErrorNotInitialized = 1;
    public const int ErrorAlreadyInitialized = 15;
    public const int ErrorBackendNotAvailable = 16;

    // Mirrors PrismConfig; bool fields are declared as byte to keep the struct blittable.
    [StructLayout(LayoutKind.Sequential)]
    public struct PrismConfig
    {
        public byte Version;
        public IntPtr Registry;
        public IntPtr AvailabilityCallback;
        public IntPtr AvailabilityUserdata;
        public uint AvailabilityPollIntervalMs;
        public uint AvailabilityDebounceSamples;
        public uint AvailabilityBackoffMaxMs;
        public byte AvailabilityAutoPowerManage;
        public IntPtr AvailabilityBaselineCallback;
    }

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    public static extern PrismConfig prism_config_init();

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr prism_init(ref PrismConfig config);

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    public static extern void prism_shutdown(IntPtr context);

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr prism_registry_acquire_best(IntPtr context);

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    public static extern int prism_backend_initialize(IntPtr backend);

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr prism_backend_name(IntPtr backend);

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    public static extern int prism_backend_output(
        IntPtr backend,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string text,
        [MarshalAs(UnmanagedType.U1)] bool interrupt);

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    public static extern int prism_backend_stop(IntPtr backend);

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    public static extern void prism_backend_free(IntPtr backend);
}
