using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using AccessKit;
using AccessKit.Speech;
using AccessKit.Text;
using AccessKit.Ui;
using MimicPartyAccess.Game;
using MimicPartyAccess.Game.Announcements;
using MimicPartyAccess.Game.Creator;
using MimicPartyAccess.Game.Screens;

namespace MimicPartyAccess;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public sealed class Plugin : BasePlugin
{
    private Harmony? _harmony;

    public override void Load()
    {
        ModLog.Initialize(Log);
        ModConfig.Bind(Config);
        MimicConfig.Bind(Config);
        var pluginDirectory = Path.GetDirectoryName(typeof(Plugin).Assembly.Location)!;

        Strings.Initialize(Path.Combine(pluginDirectory, "localization"));
        ScreenReader.LogSpeech = ModConfig.LogSpeech;
        ScreenReader.Initialize(pluginDirectory);
        LanguageWatcher.Initialize(GameLanguage.Detect);
        UiDiagnostics.Initialize(pluginDirectory);

        MenuNavigator.Initialize(new MimicUiProfile());

        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        PatchClasses(GameAnnouncementPatches.PatchClasses);
        PatchClasses(ScreenPatches.PatchClasses);
        PatchClasses(CreatorPatches.PatchClasses);
        ScreenReaders.Register();
        CreatorReaders.Register();

        ModDriver.Register(nameof(LanguageWatcher), LanguageWatcher.Tick);
        ModDriver.Register(nameof(MenuNavigator), MenuNavigator.Tick);
        ModDriver.Register(nameof(ScoreHotkey), ScoreHotkey.Tick);
        ModDriver.Register(nameof(DownloadAnnouncer), DownloadAnnouncer.Tick);
        ModDriver.Register(nameof(RecordingGuide), RecordingGuide.Tick);
        ModDriver.Register(nameof(CreatorKeys), CreatorKeys.Tick);
        ModDriver.Register(nameof(UiDiagnostics), UiDiagnostics.Tick);
        AddComponent<ModDriver>();

        ScreenReader.Say(Strings.Get("mod.loaded", MyPluginInfo.PLUGIN_NAME), interrupt: true);
        ModLog.Info($"{MyPluginInfo.PLUGIN_NAME} {MyPluginInfo.PLUGIN_VERSION} loaded.");
    }

    public override bool Unload()
    {
        _harmony?.UnpatchSelf();
        ScreenReader.Shutdown();
        return true;
    }

    private void PatchClasses(IEnumerable<Type> patchClasses)
    {
        foreach (var patchClass in patchClasses)
        {
            try
            {
                _harmony!.PatchAll(patchClass);
            }
            catch (Exception exception)
            {
                ModLog.Error($"Patch class {patchClass.Name} failed and was skipped: {exception.GetBaseException().Message}");
            }
        }
    }
}
